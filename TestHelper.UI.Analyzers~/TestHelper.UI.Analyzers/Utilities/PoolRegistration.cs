using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace TestHelper.UI.Analyzers.Utilities
{
    /// <summary>
    /// Classifies <c>Register&lt;T&gt;</c> calls on <c>OperatorPool</c> and <c>PaginatorPool</c> by the problem
    /// the pool hits when it creates an instance of <c>T</c> via reflection.
    /// </summary>
    internal sealed class PoolRegistration
    {
        private const string RegisterMethodName = "Register";

        private const string NoPublicConstructorDetail = "has no public constructor";
        private const string AbstractDetail = "is an abstract class";
        private const string InterfaceDetail = "is an interface";

        private readonly Compilation _compilation;
        private readonly IMethodSymbol _registerMethod;
        private readonly ImmutableArray<ITypeSymbol> _injectedTypes;

        private PoolRegistration(Compilation compilation, IMethodSymbol registerMethod,
            ImmutableArray<ITypeSymbol> injectedTypes)
        {
            _compilation = compilation;
            _registerMethod = registerMethod;
            _injectedTypes = injectedTypes;
        }

        /// <summary>
        /// Registers an action that reports <paramref name="descriptor"/> at each <c>Register&lt;T&gt;</c> call
        /// on the pool whose highest-priority problem is <paramref name="rule"/>.
        /// </summary>
        /// <param name="context">The analyzer's context</param>
        /// <param name="poolMetadataName">The metadata name of the pool type</param>
        /// <param name="rule">The rule to report</param>
        /// <param name="descriptor">The descriptor to report</param>
        public static void RegisterRuleAction(AnalysisContext context, string poolMetadataName,
            PoolRegistrationRule rule, DiagnosticDescriptor descriptor)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var registration = Create(compilationContext.Compilation, poolMetadataName);
                if (registration == null)
                {
                    return;
                }

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    var invocation = (IInvocationOperation)operationContext.Operation;
                    var (actual, messageArgs) = registration.Classify(invocation, operationContext.CancellationToken);
                    if (actual == rule)
                    {
                        operationContext.ReportDiagnostic(
                            Diagnostic.Create(descriptor, GetRegisterNameLocation(invocation), messageArgs));
                    }
                }, OperationKind.Invocation);
            });
        }

        private static PoolRegistration? Create(Compilation compilation, string poolMetadataName)
        {
            var poolType = compilation.GetTypeByMetadataName(poolMetadataName);
            if (poolType == null)
            {
                return null;
            }

            IMethodSymbol? registerMethod = null;
            foreach (var member in poolType.GetMembers(RegisterMethodName))
            {
                if (member is IMethodSymbol { IsGenericMethod: true, Parameters.Length: 1 } method)
                {
                    registerMethod = method;
                }
            }

            return registerMethod == null
                ? null
                : new PoolRegistration(compilation, registerMethod, GetInjectedTypes(poolType));
        }

        /// <summary>
        /// The parameter types whose values the pool takes in its constructor and injects into the pooled type's constructor.
        /// </summary>
        private static ImmutableArray<ITypeSymbol> GetInjectedTypes(INamedTypeSymbol poolType)
        {
            // A hard-coded list of OperatorPool.ResolveArgument's types is rejected: it would miss a type added to the
            // pool later, and 4002 at Error severity would then break builds that work.
            // Value-type parameters are skipped because they are options of the pool itself (requireRegistration).
            var builder = ImmutableArray.CreateBuilder<ITypeSymbol>();
            foreach (var constructor in poolType.InstanceConstructors)
            {
                if (constructor.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                foreach (var parameter in constructor.Parameters)
                {
                    if (!parameter.Type.IsValueType)
                    {
                        builder.Add(parameter.Type);
                    }
                }
            }

            return builder.ToImmutable();
        }

        private (PoolRegistrationRule Rule, object[] MessageArgs) Classify(IInvocationOperation invocation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var method = invocation.TargetMethod;
            // Checking the name first is cheaper than the symbol comparison, and this runs for every call in the compilation.
            if (!string.Equals(method.Name, RegisterMethodName, StringComparison.Ordinal) ||
                !SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, _registerMethod))
            {
                return (PoolRegistrationRule.None, Array.Empty<object>());
            }

            // A type parameter is rejected: its actual type is unknown until the generic method is constructed.
            if (!(method.TypeArguments[0] is INamedTypeSymbol type) || type.TypeKind == TypeKind.Error)
            {
                return (PoolRegistrationRule.None, Array.Empty<object>());
            }

            if (type.TypeKind == TypeKind.Interface)
            {
                return (PoolRegistrationRule.NoPublicConstructor, new object[] { type.Name, InterfaceDetail });
            }

            if (type.IsAbstract)
            {
                return (PoolRegistrationRule.NoPublicConstructor, new object[] { type.Name, AbstractDetail });
            }

            var publicConstructorCount = PooledTypeSymbols.CountPublicConstructors(type, out var firstConstructor);
            if (publicConstructorCount == 0)
            {
                return (PoolRegistrationRule.NoPublicConstructor, new object[] { type.Name, NoPublicConstructorDetail });
            }

            var isTypePreserved = PooledTypeSymbols.HasPreserveAttribute(type);
            var arguments = GetInspectableArguments(invocation);
            if (arguments is { IsEmpty: true })
            {
                if (publicConstructorCount > 1)
                {
                    return (PoolRegistrationRule.MultiplePublicConstructors, new object[] { type.Name });
                }

                var requiredParameter = FindRequiredParameter(firstConstructor!);
                if (requiredParameter != null)
                {
                    return (PoolRegistrationRule.RequiredParameter,
                        new object[] { requiredParameter.Name, requiredParameter.Type.Name });
                }
            }

            return (ClassifyConstructors(type, arguments, isTypePreserved, cancellationToken), new object[] { type.Name });
        }

        /// <summary>
        /// Classifies the public constructors of <paramref name="type"/> that the arguments might bind to.
        /// </summary>
        /// <param name="type">The registered type</param>
        /// <param name="arguments">The arguments to match, or null to treat every public constructor as matching</param>
        /// <param name="isTypePreserved">Whether <paramref name="type"/> has a <c>[Preserve]</c> attribute</param>
        /// <param name="cancellationToken">The cancellation token of the analysis</param>
        private PoolRegistrationRule ClassifyConstructors(INamedTypeSymbol type, ImmutableArray<IOperation>? arguments,
            bool isTypePreserved, CancellationToken cancellationToken)
        {
            var hasMatch = false;
            foreach (var constructor in type.InstanceConstructors)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (constructor.DeclaredAccessibility != Accessibility.Public ||
                    (arguments is { } args && !CanMatch(constructor, args)))
                {
                    continue;
                }

                if (!IsPreserved(constructor, isTypePreserved))
                {
                    return PoolRegistrationRule.NotPreserved;
                }

                hasMatch = true;
            }

            return hasMatch ? PoolRegistrationRule.None : PoolRegistrationRule.NoMatchingConstructor;
        }

        private static Location GetRegisterNameLocation(IInvocationOperation invocation)
        {
            // Reporting at the whole invocation is rejected: in a fluent chain, the invocation of the second Register
            // spans the first one, so the locations would overlap.
            var name = (invocation.Syntax as InvocationExpressionSyntax)?.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
                MemberBindingExpressionSyntax memberBinding => memberBinding.Name,
                SimpleNameSyntax simpleName => simpleName,
                _ => null
            };
            return (name ?? invocation.Syntax).GetLocation();
        }

        /// <summary>
        /// Extracts the constructor arguments of a <c>Register&lt;T&gt;</c> call.
        /// </summary>
        /// <returns>
        /// The arguments written in the call (empty for no arguments), or null when the call passes an array expression
        /// whose elements are unknown at compile time
        /// </returns>
        private static ImmutableArray<IOperation>? GetInspectableArguments(IInvocationOperation invocation)
        {
            foreach (var argument in invocation.Arguments)
            {
                if (argument.ArgumentKind == ArgumentKind.ParamArray &&
                    argument.Value is IArrayCreationOperation { Initializer: { } initializer })
                {
                    return initializer.ElementValues;
                }

                // The pool treats a null args array the same as no arguments (Register<T>(null), Register<T>(default)).
                if (argument.Value.ConstantValue is { HasValue: true, Value: null })
                {
                    return ImmutableArray<IOperation>.Empty;
                }
            }

            return null;
        }

        private IParameterSymbol? FindRequiredParameter(IMethodSymbol constructor)
        {
            foreach (var parameter in constructor.Parameters)
            {
                // IsOptional is rejected: a parameter with only [Optional] has no default value for reflection
                // (ParameterInfo.HasDefaultValue is false), so the pool cannot resolve it either.
                if (!parameter.HasExplicitDefaultValue && !IsInjected(parameter.Type))
                {
                    return parameter;
                }
            }

            return null;
        }

        private bool IsInjected(ITypeSymbol type)
        {
            // Reporting injectable types is rejected: whether the pool holds a value for them depends on how the
            // caller constructed the pool, which is unknown at the Register<T> call.
            foreach (var injected in _injectedTypes)
            {
                if (SymbolEqualityComparer.Default.Equals(type, injected))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPreserved(IMethodSymbol constructor, bool isTypePreserved)
        {
            return PooledTypeSymbols.HasPreserveAttribute(constructor) ||
                   (isTypePreserved && constructor.Parameters.IsEmpty);
        }

        /// <summary>
        /// Whether <c>Activator.CreateInstance</c> might bind <paramref name="arguments"/> to <paramref name="constructor"/>.
        /// </summary>
        /// <remarks>
        /// Reproducing the binding rules of the runtime's <c>DefaultBinder</c> is rejected: they differ between Mono and IL2CPP.
        /// This returns false only when the call certainly fails, so that an Error diagnostic never breaks a working build.
        /// </remarks>
        private bool CanMatch(IMethodSymbol constructor, ImmutableArray<IOperation> arguments)
        {
            var parameters = constructor.Parameters;
            // DefaultBinder expands a params array, so any arguments might bind to it.
            if (!parameters.IsEmpty && parameters[parameters.Length - 1].IsParams)
            {
                return true;
            }

            if (arguments.Length > parameters.Length)
            {
                return false;
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                if (IsCertainlyIncompatible(arguments[i], parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsCertainlyIncompatible(IOperation argument, ITypeSymbol parameterType)
        {
            // Each element is implicitly converted to object; the static type of the source expression is the one that matters.
            while (argument is IConversionOperation { IsImplicit: true } conversion)
            {
                argument = conversion.Operand;
            }

            if (argument.ConstantValue is { HasValue: true, Value: null })
            {
                return parameterType.IsValueType &&
                       parameterType.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T;
            }

            var argumentType = argument.Type;
            // Only a struct or sealed class has a runtime type equal to its static type. Any other static type
            // (interface, non-sealed class, object) may hold an instance that matches the parameter.
            if (argumentType == null ||
                !(argumentType.TypeKind == TypeKind.Struct ||
                  argumentType.TypeKind == TypeKind.Enum ||
                  (argumentType.TypeKind == TypeKind.Class && argumentType.IsSealed)))
            {
                return false;
            }

            // Runtime binders differ on primitive conversions of enums.
            if (argumentType.TypeKind == TypeKind.Enum && IsIntegral(parameterType))
            {
                return false;
            }

            var conversionToParameter = _compilation.ClassifyConversion(argumentType, parameterType);
            // A user-defined conversion is not accepted: reflection never calls op_Implicit.
            return !(conversionToParameter.IsIdentity ||
                     conversionToParameter.IsBoxing ||
                     (conversionToParameter.IsImplicit &&
                      (conversionToParameter.IsReference ||
                       conversionToParameter.IsNumeric ||
                       conversionToParameter.IsNullable)));
        }

        private static bool IsIntegral(ITypeSymbol type)
        {
            return type.SpecialType switch
            {
                SpecialType.System_SByte or SpecialType.System_Byte or
                    SpecialType.System_Int16 or SpecialType.System_UInt16 or
                    SpecialType.System_Int32 or SpecialType.System_UInt32 or
                    SpecialType.System_Int64 or SpecialType.System_UInt64 or
                    SpecialType.System_Char => true,
                _ => false
            };
        }
    }
}
