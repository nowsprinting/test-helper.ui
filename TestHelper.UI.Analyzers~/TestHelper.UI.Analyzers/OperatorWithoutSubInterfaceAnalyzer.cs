using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorWithoutSubInterfaceAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4005";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IOperator implementation does not implement a sub-interface",
            messageFormat:
            "'{0}' implements IOperator but no sub-interface: callers cannot refer to the operator by the kind of operation. Implement a sub-interface such as 'IClickOperator'.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description:
            "A sub-interface of IOperator represents the kind of operation, and it is the type that test code depends on to swap operator implementations.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4005.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
        }
    }
}
