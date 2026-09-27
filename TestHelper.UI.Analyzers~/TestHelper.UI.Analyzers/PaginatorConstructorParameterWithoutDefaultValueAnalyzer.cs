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
            title: "Constructor parameter of IPaginator implementation has no default value",
            messageFormat:
            "Parameter '{0}' of the '{1}' constructor has no default value: the paginator cannot be rented unless it is registered with constructor arguments. Add a default value.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "PaginatorPool fills constructor parameters only with their default values, so renting a paginator with a parameter without one throws unless it is registered with constructor arguments.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4007.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var paginatorType =
                    compilationContext.Compilation.GetTypeByMetadataName(PooledTypeSymbols.IPaginatorMetadataName);
                if (paginatorType == null)
                {
                    return;
                }

                compilationContext.RegisterSymbolAction(
                    symbolContext => AnalyzeNamedType(symbolContext, paginatorType),
                    SymbolKind.NamedType);
            });
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context, INamedTypeSymbol paginatorType)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var type = (INamedTypeSymbol)context.Symbol;
            // Reporting the parameters of multiple public constructors is rejected: PaginatorPool resolves parameters
            // only for a single one, and TestHelperUI4009 reports the multiple constructors instead.
            if (!PooledTypeSymbols.IsConcreteImplementation(type, paginatorType) ||
                PooledTypeSymbols.CountPublicConstructors(type, out var publicConstructor) != 1)
            {
                return;
            }

            foreach (var parameter in publicConstructor!.Parameters)
            {
                // IsOptional is rejected: a parameter with only [Optional] has no default value for reflection
                // (ParameterInfo.HasDefaultValue is false), so PaginatorPool cannot resolve it either.
                if (!parameter.HasExplicitDefaultValue)
                {
                    context.ReportDiagnostic(
                        Diagnostic.Create(s_rule, parameter.Locations[0], parameter.Name, type.Name));
                }
            }
        }
    }
}
