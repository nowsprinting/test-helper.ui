using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorConstructorParameterWithoutDefaultValueAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4002";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Constructor parameter of IOperator registered without arguments cannot be resolved",
            messageFormat:
            "Cannot resolve required parameter '{0}' of type {1}. Register with explicit constructor arguments or add a default value.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "When registered without constructor arguments, OperatorPool fills each parameter of the single public constructor with a value injected into the pool or its default value, so renting throws when a parameter has neither. Parameters of the injected types are not reported because whether the pool holds the value is unknown at the call.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4002.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            PoolRegistration.RegisterRuleAction(context, PooledTypeSymbols.OperatorPoolMetadataName,
                PoolRegistrationRule.RequiredParameter, s_rule);
        }
    }
}
