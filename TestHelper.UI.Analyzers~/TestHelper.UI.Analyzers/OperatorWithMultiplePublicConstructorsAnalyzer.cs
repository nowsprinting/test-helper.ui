using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorWithMultiplePublicConstructorsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4004";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IOperator implementation has multiple public constructors",
            messageFormat:
            "'{0}' has multiple public constructors: the operator cannot be rented unless it is registered with constructor arguments. Keep only one public constructor.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "OperatorPool does not choose among multiple public constructors, so renting such an operator throws unless it is registered with explicit constructor arguments.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4004.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
