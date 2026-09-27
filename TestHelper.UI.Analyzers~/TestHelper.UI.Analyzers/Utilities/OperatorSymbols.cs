using Microsoft.CodeAnalysis;

namespace TestHelper.UI.Analyzers.Utilities
{
    internal static class OperatorSymbols
    {
        public const string IOperatorMetadataName = "TestHelper.UI.Operators.IOperator";

        /// <summary>
        /// Whether <paramref name="type"/> is a class that <c>OperatorPool</c> can instantiate as an operator:
        /// non-abstract and implementing <c>IOperator</c> directly, through a sub-interface, or through a base class.
        /// </summary>
        public static bool IsConcreteOperator(INamedTypeSymbol type, INamedTypeSymbol operatorType)
        {
            if (type.TypeKind != TypeKind.Class || type.IsAbstract)
            {
                return false;
            }

            // LINQ Any is rejected for boxing the ImmutableArray enumerator on every call; this runs for every class.
            foreach (var implemented in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, operatorType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
