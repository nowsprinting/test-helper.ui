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
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var operatorType =
                    compilationContext.Compilation.GetTypeByMetadataName(OperatorSymbols.IOperatorMetadataName);
                if (operatorType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(
                    symbolContext => AnalyzeNamedType(symbolContext, operatorType),
                    SymbolKind.NamedType);
            });
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol operatorType)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var type = (INamedTypeSymbol)context.Symbol;
            // Reporting the parameters of multiple public constructors is rejected: OperatorPool resolves parameters
            // only for a single one, and TestHelperUI4004 reports the multiple constructors instead.
            if (!OperatorSymbols.IsConcreteOperator(type, operatorType) ||
                OperatorSymbols.CountPublicConstructors(type, out var publicConstructor) != 1)
            {
                return;
            }

            foreach (var parameter in publicConstructor!.Parameters)
            {
                // IsOptional is rejected: a parameter with only [Optional] has no default value for reflection
                // (ParameterInfo.HasDefaultValue is false), so OperatorPool cannot resolve it either.
                if (!parameter.HasExplicitDefaultValue)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(s_rule, parameter.Locations[0], parameter.Name, type.Name));
                }
            }
        }
    }
}
