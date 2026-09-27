using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PaginatorWithoutGenericInterfaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4010";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IPaginator implementation does not implement IPaginator<TComponent>",
            messageFormat:
            "'{0}' implements IPaginator but not IPaginator<TComponent>: the paginator cannot be rented by its target component. Implement 'IPaginator<TComponent>' instead.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "PaginatorPool.Rent(targetComponent) selects a paginator by the TComponent of its IPaginator<TComponent>, so it never selects a paginator that implements only IPaginator.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4010.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
