using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class OperatorConstructorParameterWithoutDefaultValueAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4002";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Constructor parameter of IOperator implementation has no default value",
            messageFormat:
            "Parameter '{0}' of the '{1}' constructor has no default value: the operator cannot be rented unless the pool injects the value or the operator is registered with constructor arguments. Add a default value.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "OperatorPool fills a constructor parameter without a default value only when the pool holds a value to inject for its type, so the operator depends on how the caller configures the pool.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4002.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
        }
    }
}
