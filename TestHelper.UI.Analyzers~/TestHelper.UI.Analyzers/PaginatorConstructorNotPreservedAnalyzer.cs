using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper.UI.Analyzers.Utilities;

namespace TestHelper.UI.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PaginatorConstructorNotPreservedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TestHelperUI4008";

        private static readonly DiagnosticDescriptor s_rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: "Public constructor of IPaginator implementation is not preserved",
            messageFormat:
            "The public constructor of '{0}' is not preserved: managed code stripping can remove the constructor from the Player, and the paginator cannot be rented there. Apply 'Preserve' to the constructor.",
            category: "Extensibility",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:
            "PaginatorPool invokes the constructor via reflection, so the Unity linker cannot see the call and can strip the constructor from the Player unless it is preserved.",
            helpLinkUri:
            "https://github.com/nowsprinting/test-helper.ui/tree/master/Documentation~/rules/TestHelperUI4008.md");

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
            if (!PooledTypeSymbols.IsConcreteImplementation(type, paginatorType))
            {
                return;
            }

            var isTypePreserved = PooledTypeSymbols.HasPreserveAttribute(type);
            // LINQ is rejected for boxing the ImmutableArray enumerator on every call.
            foreach (var constructor in type.InstanceConstructors)
            {
                if (constructor.DeclaredAccessibility != Accessibility.Public ||
                    PooledTypeSymbols.HasPreserveAttribute(constructor) ||
                    (isTypePreserved && constructor.Parameters.IsEmpty))
                {
                    continue;
                }

                // The implicit default constructor has no syntax to point at, and the fix may go on the class.
                var location = constructor.IsImplicitlyDeclared ? type.Locations[0] : constructor.Locations[0];
                context.ReportDiagnostic(Diagnostic.Create(s_rule, location, type.Name));
            }
        }
    }
}
