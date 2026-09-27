using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RentAllWithoutListAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI3002";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "OperatorPool.RentAll is called without a list",
            messageFormat:
            "'OperatorPool.RentAll' is called without a list: a new list is allocated on every call. Pass a list to reuse.",
            category: "Performance",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description:
            "OperatorPool.RentAll allocates a new list on every call when no list is passed, which produces GC garbage when the call is repeated.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI3002.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
