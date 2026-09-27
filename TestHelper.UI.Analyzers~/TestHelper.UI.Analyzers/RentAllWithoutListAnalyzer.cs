using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class RentAllWithoutListAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI3002";

        private const string RentAllMethodName = "RentAll";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "OperatorPool.RentAll is called without a list",
            messageFormat:
            "'OperatorPool.RentAll' is called without a list: a new list is allocated on every call. Pass a list to reuse.",
            category: "Performance",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description:
            "OperatorPool.RentAll allocates a new list on every call when no list is passed, which produces GC garbage when the call is repeated.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI3002.md");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(s_rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(compilationContext =>
            {
                var operatorPoolType =
                    compilationContext.Compilation.GetTypeByMetadataName(PooledTypeSymbols.OperatorPoolMetadataName);
                if (operatorPoolType == null)
                {
                    return;
                }

                var rentAllMethods = operatorPoolType.GetMembers(RentAllMethodName);
                if (rentAllMethods.IsEmpty)
                {
                    return;
                }

                compilationContext.RegisterOperationAction(
                    operationContext => AnalyzeInvocation(operationContext, rentAllMethods),
                    OperationKind.Invocation);
            });
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, ImmutableArray<ISymbol> rentAllMethods)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var invocation = (IInvocationOperation)context.Operation;
            if (!IsAnyOf(invocation.TargetMethod.OriginalDefinition, rentAllMethods))
            {
                return;
            }

            foreach (var argument in invocation.Arguments)
            {
                // Checking ArgumentKind.DefaultValue alone is rejected: an explicit null or default argument
                // allocates the same way. An omitted argument also carries its constant null default value.
                if (argument.Value.ConstantValue is { HasValue: true, Value: null })
                {
                    context.ReportDiagnostic(Diagnostic.Create(s_rule, invocation.Syntax.GetLocation()));
                    return;
                }
            }
        }

        private static bool IsAnyOf(ISymbol symbol, ImmutableArray<ISymbol> candidates)
        {
            // LINQ Contains is rejected for boxing the ImmutableArray enumerator on every invocation.
            foreach (var candidate in candidates)
            {
                if (SymbolEqualityComparer.Default.Equals(symbol, candidate))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
