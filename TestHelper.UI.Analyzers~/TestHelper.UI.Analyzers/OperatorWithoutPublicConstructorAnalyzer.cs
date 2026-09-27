using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

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
            "OperatorPool creates operators only through a public constructor, so creating an instance of an operator whose constructors are all non-public always throws.",
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
            if (!PooledTypeSymbols.IsConcreteImplementation(type, operatorType) ||
                PooledTypeSymbols.CountPublicConstructors(type, out _) > 0)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(s_rule, type.Locations[0], type.Name));
        }
    }
}
