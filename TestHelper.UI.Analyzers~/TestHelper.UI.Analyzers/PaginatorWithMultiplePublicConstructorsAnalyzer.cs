using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PaginatorWithMultiplePublicConstructorsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4009";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IPaginator implementation has multiple public constructors",
            messageFormat:
            "'{0}' has multiple public constructors: the paginator cannot be rented unless it is registered with constructor arguments. Keep only one public constructor.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "PaginatorPool refuses to choose among multiple public constructors, so renting such a paginator throws unless it is registered with explicit constructor arguments.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4009.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
