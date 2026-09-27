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
            title: "Constructor of IOperator registered to OperatorPool is not preserved",
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
            PoolRegistration.RegisterRuleAction(context, PooledTypeSymbols.OperatorPoolMetadataName,
                PoolRegistrationRule.NotPreserved, s_rule);
        }
    }
}
