using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorRegisterArgumentsMatchNoConstructorAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4011";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "No public constructor of IOperator matches the arguments of OperatorPool.Register",
            messageFormat: "No public constructor of '{0}' matches the arguments",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "OperatorPool creates the operator with Activator.CreateInstance and the registered arguments, so renting it throws MissingMethodException when no public constructor accepts them.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4011.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            PoolRegistration.RegisterRuleAction(context, PooledTypeSymbols.OperatorPoolMetadataName,
                PoolRegistrationRule.NoMatchingConstructor, s_rule);
        }
    }
}
