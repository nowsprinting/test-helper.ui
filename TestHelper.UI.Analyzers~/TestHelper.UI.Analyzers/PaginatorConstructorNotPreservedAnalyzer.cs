using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PaginatorConstructorNotPreservedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4008";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Public constructor of IPaginator implementation is not preserved",
            messageFormat:
            "The public constructor of '{0}' is not preserved: managed code stripping can remove the constructor from the Player, and the paginator cannot be rented there. Apply 'Preserve' to the constructor.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "PaginatorPool invokes the constructor via reflection, so the Unity linker cannot see the call and can strip the constructor from the Player unless it is preserved.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4008.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
