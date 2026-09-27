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
        /// <remarks>
        /// Call this before <see cref="CountPublicConstructors"/>. Checking the constructors first is rejected:
        /// <c>InstanceConstructors</c> builds a new array on every access while <c>AllInterfaces</c> is cached,
        /// so rejecting non-operators by interface first is about twice as fast.
        /// </remarks>
        public static bool IsConcreteOperator(INamedTypeSymbol type, INamedTypeSymbol operatorType)
        {
            return type.TypeKind == TypeKind.Class && !type.IsAbstract && InheritsOperator(type, operatorType);
        }

        /// <summary>
        /// Whether <paramref name="type"/> implements or inherits <c>IOperator</c>, directly or indirectly.
        /// </summary>
        public static bool InheritsOperator(ITypeSymbol type, INamedTypeSymbol operatorType)
        {
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

        /// <summary>
        /// Counts the public instance constructors, which are the ones <c>OperatorPool</c> can invoke.
        /// </summary>
        /// <param name="type">The operator type</param>
        /// <param name="first">The first public constructor, or null if there is none</param>
        /// <returns>The number of public instance constructors, including the implicit default constructor</returns>
        public static int CountPublicConstructors(INamedTypeSymbol type, out IMethodSymbol? first)
        {
            first = null;
            var count = 0;
            // LINQ is rejected for boxing the ImmutableArray enumerator on every call.
            foreach (var constructor in type.InstanceConstructors)
            {
                if (constructor.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                first ??= constructor;
                count++;
            }

            return count;
        }
    }
}
