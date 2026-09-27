using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorWithMultiplePublicConstructorsAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4004";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "IOperator with multiple public constructors is registered without arguments",
            messageFormat:
            "'{0}' has multiple public constructors. Register with explicit constructor arguments.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "OperatorPool does not choose among multiple public constructors, so renting a operator registered without constructor arguments always throws.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4004.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            PoolRegistration.RegisterRuleAction(context, PoolRegistration.OperatorPoolMetadataName,
                injectsParameters: true, PoolRegistrationRule.MultiplePublicConstructors, s_rule);
        }
    }
}
