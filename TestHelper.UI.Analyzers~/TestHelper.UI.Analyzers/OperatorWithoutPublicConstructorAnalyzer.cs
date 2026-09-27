using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorWithoutPublicConstructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4001";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IOperator implementation has no public constructor",
            messageFormat:
            "'{0}' has no public constructor: the operator cannot be rented. Make the constructor public.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "OperatorPool creates operators only through a public constructor, so renting an operator whose constructors are all non-public always throws.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4001.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var operatorType =
                    compilationContext.Compilation.GetTypeByMetadataName("TestHelper.UI.Operators.IOperator");
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
            // Checking the constructors before the interfaces is rejected: InstanceConstructors builds a new array on
            // every access while AllInterfaces is cached, so rejecting non-operators by interface first is about twice as
            // fast. LINQ Any is rejected for boxing the ImmutableArray enumerator on every type.
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || !ImplementsOperator(type, operatorType))
            {
                return;
            }

            foreach (var constructor in type.InstanceConstructors)
            {
                if (constructor.DeclaredAccessibility == Accessibility.Public)
                {
                    return;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(s_rule, type.Locations[0], type.Name));
        }

        private static bool ImplementsOperator(INamedTypeSymbol type, INamedTypeSymbol operatorType)
        {
            foreach (var implemented in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, operatorType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
