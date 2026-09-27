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
        public const string OperatorPoolMetadataName = "TestHelper.UI.OperatorPool";
        public const string PaginatorPoolMetadataName = "TestHelper.UI.PaginatorPool";

        private const string RegisterMethodName = "Register";

        private const string NoPublicConstructorDetail = "has no public constructor";
        private const string AbstractDetail = "is abstract";

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
        /// <param name="injectsParameters">Whether the pool injects its own values into constructor parameters (<c>OperatorPool</c> does)</param>
        /// <param name="rule">The rule to report</param>
        /// <param name="descriptor">The descriptor to report</param>
        public static void RegisterRuleAction(AnalysisContext context, string poolMetadataName, bool injectsParameters,
            PoolRegistrationRule rule, DiagnosticDescriptor descriptor)
        {
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var registration = Create(compilationContext.Compilation, poolMetadataName, injectsParameters);
                if (registration == null)
                {
                    return;
                }

                compilationContext.RegisterOperationAction(operationContext =>
                {
                    var actual = registration.Classify(
                        (IInvocationOperation)operationContext.Operation, operationContext.CancellationToken,
                        out var location, out var messageArgs);
                    if (actual == rule)
                    {
                        operationContext.ReportDiagnostic(Diagnostic.Create(descriptor, location, messageArgs));
                    }
                }, OperationKind.Invocation);
            });
        }

        private static PoolRegistration? Create(Compilation compilation, string poolMetadataName,
            bool injectsParameters)
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

            if (registerMethod == null)
            {
                return null;
            }

            return new PoolRegistration(compilation, registerMethod,
                injectsParameters ? GetInjectedTypes(compilation) : ImmutableArray<ITypeSymbol>.Empty);
        }

        /// <summary>
        /// The parameter types that <c>OperatorPool.ResolveArgument</c> fills with the values given to the pool's constructor.
        /// </summary>
        private static ImmutableArray<ITypeSymbol> GetInjectedTypes(Compilation compilation)
        {
            var builder = ImmutableArray.CreateBuilder<ITypeSymbol>();
            foreach (var metadataName in new[]
                     {
                         "UnityEngine.ILogger",
                         "TestHelper.UI.ScreenshotOptions",
                         "TestHelper.UI.Visualizers.IVisualizer",
                         "TestHelper.UI.Strategies.IReachableStrategy",
                         "TestHelper.Random.IRandom"
                     })
            {
                var type = compilation.GetTypeByMetadataName(metadataName);
                if (type != null)
                {
                    builder.Add(type);
                }
            }

            var func = compilation.GetTypeByMetadataName("System.Func`2");
            var gameObject = compilation.GetTypeByMetadataName("UnityEngine.GameObject");
            var vector2 = compilation.GetTypeByMetadataName("UnityEngine.Vector2");
            if (func != null && gameObject != null && vector2 != null)
            {
                builder.Add(func.Construct(gameObject, vector2));
            }

            return builder.ToImmutable();
        }

        private PoolRegistrationRule Classify(IInvocationOperation invocation, CancellationToken cancellationToken,
            out Location? location, out object[] messageArgs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            location = null;
            messageArgs = System.Array.Empty<object>();

            var method = invocation.TargetMethod;
            // Checking the name first is cheaper than the symbol comparison, and this runs for every call in the compilation.
            if (!string.Equals(method.Name, RegisterMethodName, System.StringComparison.Ordinal) ||
                !method.IsGenericMethod ||
                !SymbolEqualityComparer.Default.Equals(method.OriginalDefinition, _registerMethod))
            {
                return PoolRegistrationRule.None;
            }

            // A type parameter is rejected: its actual type is unknown until the generic method is constructed.
            if (!(method.TypeArguments[0] is INamedTypeSymbol type) || type.TypeKind == TypeKind.Error)
            {
                return PoolRegistrationRule.None;
            }

            location = GetRegisterNameLocation(invocation);
            var typeName = type.Name;
            var isAbstractClass = type.TypeKind == TypeKind.Class && type.IsAbstract;
            var publicConstructorCount = PooledTypeSymbols.CountPublicConstructors(type, out var firstConstructor);
            if (isAbstractClass || publicConstructorCount == 0)
            {
                messageArgs = new object[] { typeName, isAbstractClass ? AbstractDetail : NoPublicConstructorDetail };
                return PoolRegistrationRule.NoPublicConstructor;
            }

            messageArgs = new object[] { typeName };
            var isTypePreserved = PooledTypeSymbols.HasPreserveAttribute(type);
            switch (ClassifyArguments(invocation, out var elements))
            {
                case ArgumentShape.None:
                    return ClassifyWithoutArguments(publicConstructorCount, firstConstructor!, isTypePreserved,
                        ref messageArgs);
                case ArgumentShape.Inspectable:
                    return ClassifyWithArguments(type, elements, isTypePreserved, cancellationToken);
                case ArgumentShape.Uninspectable:
                default:
                    return HasNotPreservedPublicConstructor(type, isTypePreserved)
                        ? PoolRegistrationRule.NotPreserved
                        : PoolRegistrationRule.None;
            }
        }

        private PoolRegistrationRule ClassifyWithoutArguments(int publicConstructorCount, IMethodSymbol constructor,
            bool isTypePreserved, ref object[] messageArgs)
        {
            if (publicConstructorCount > 1)
            {
                return PoolRegistrationRule.MultiplePublicConstructors;
            }

            var requiredParameter = FindRequiredParameter(constructor);
            if (requiredParameter != null)
            {
                messageArgs = new object[] { requiredParameter.Name, requiredParameter.Type.Name };
                return PoolRegistrationRule.RequiredParameter;
            }

            return IsPreserved(constructor, isTypePreserved)
                ? PoolRegistrationRule.None
                : PoolRegistrationRule.NotPreserved;
        }

        private PoolRegistrationRule ClassifyWithArguments(INamedTypeSymbol type, ImmutableArray<IOperation> arguments,
            bool isTypePreserved, CancellationToken cancellationToken)
        {
            var hasMatch = false;
            foreach (var constructor in type.InstanceConstructors)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (constructor.DeclaredAccessibility != Accessibility.Public || !CanMatch(constructor, arguments))
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

        private static bool HasNotPreservedPublicConstructor(INamedTypeSymbol type, bool isTypePreserved)
        {
            foreach (var constructor in type.InstanceConstructors)
            {
                if (constructor.DeclaredAccessibility == Accessibility.Public &&
                    !IsPreserved(constructor, isTypePreserved))
                {
                    return true;
                }
            }

            return false;
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

        private enum ArgumentShape
        {
            None,
            Inspectable,
            Uninspectable
        }

        private static ArgumentShape ClassifyArguments(IInvocationOperation invocation,
            out ImmutableArray<IOperation> elements)
        {
            elements = ImmutableArray<IOperation>.Empty;
            foreach (var argument in invocation.Arguments)
            {
                if (argument.ArgumentKind == ArgumentKind.ParamArray &&
                    argument.Value is IArrayCreationOperation { Initializer: { } initializer })
                {
                    elements = initializer.ElementValues;
                    return elements.IsEmpty ? ArgumentShape.None : ArgumentShape.Inspectable;
                }

                // The pool treats a null args array the same as no arguments (Register<T>(null), Register<T>(default)).
                if (argument.Value.ConstantValue is { HasValue: true, Value: null })
                {
                    return ArgumentShape.None;
                }
            }

            return ArgumentShape.Uninspectable;
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
