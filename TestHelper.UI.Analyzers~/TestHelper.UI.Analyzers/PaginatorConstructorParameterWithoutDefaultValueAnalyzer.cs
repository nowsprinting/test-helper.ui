using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PaginatorConstructorParameterWithoutDefaultValueAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4007";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Constructor parameter of IPaginator registered without arguments cannot be resolved",
            messageFormat:
            "Cannot resolve required parameter '{0}' of type {1}. Register with explicit constructor arguments or add a default value.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "When registered without constructor arguments, PaginatorPool fills each parameter of the single public constructor with its default value, so renting throws when a parameter has none.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4007.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            PoolRegistration.RegisterRuleAction(context, PoolRegistration.PaginatorPoolMetadataName,
                injectsParameters: false, PoolRegistrationRule.RequiredParameter, s_rule);
        }
    }
}
