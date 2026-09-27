using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorConstructorNotPreservedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4003";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Public constructor of IOperator implementation is not preserved",
            messageFormat:
            "The public constructor of '{0}' is not preserved: managed code stripping can remove the constructor from the Player, and the operator cannot be rented there. Apply 'Preserve' to the constructor.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "OperatorPool invokes the constructor only via reflection, so the Unity linker cannot see the call and strips the constructor from the Player unless it is preserved.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4003.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var operatorType =
                    compilationContext.Compilation.GetTypeByMetadataName(PooledTypeSymbols.IOperatorMetadataName);
                if (operatorType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(
                    symbolContext => AnalyzeNamedType(symbolContext, operatorType),
                    SymbolKind.NamedType);
            });
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol operatorType)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var type = (INamedTypeSymbol)context.Symbol;
            if (!PooledTypeSymbols.IsConcreteImplementation(type, operatorType))
            {
                return;
            }

            var isTypePreserved = HasPreserveAttribute(type);
            // LINQ is rejected for boxing the ImmutableArray enumerator on every call.
            foreach (var constructor in type.InstanceConstructors)
            {
                if (constructor.DeclaredAccessibility != Accessibility.Public ||
                    HasPreserveAttribute(constructor) ||
                    (isTypePreserved && constructor.Parameters.IsEmpty))
                {
                    continue;
                }

                // The implicit default constructor has no syntax to point at, and the fix may go on the class.
                var location = constructor.IsImplicitlyDeclared ? type.Locations[0] : constructor.Locations[0];
                context.ReportDiagnostic(Diagnostic.Create(s_rule, location, type.Name));
            }
        }

        private static bool HasPreserveAttribute(ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                // Matching UnityEngine.Scripting.PreserveAttribute by symbol is rejected: the Unity linker recognizes
                // any attribute class named PreserveAttribute in any namespace, or deriving from one.
                for (var attributeClass = attribute.AttributeClass;
                     attributeClass != null;
                     attributeClass = attributeClass.BaseType)
                {
                    if (string.Equals(attributeClass.Name, "PreserveAttribute", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
