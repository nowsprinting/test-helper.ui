using System;
using Microsoft.CodeAnalysis;

namespace TestHelper.UI.Analyzers.Utilities
{
    /// <summary>
    /// Symbol helpers for the types that <c>OperatorPool</c> and <c>PaginatorPool</c> instantiate via reflection,
    /// including whether the Unity linker keeps their constructors.
    /// </summary>
    internal static class PooledTypeSymbols
    {
        public const string IOperatorMetadataName = "TestHelper.UI.Operators.IOperator";
        public const string IPaginatorMetadataName = "TestHelper.UI.Paginators.IPaginator";
        public const string GenericIPaginatorMetadataName = "TestHelper.UI.Paginators.IPaginator`1";

        /// <summary>
        /// Whether <paramref name="type"/> is a class that a pool can instantiate as <paramref name="interfaceType"/>:
        /// non-abstract and implementing <paramref name="interfaceType"/> directly, through a sub-interface, or through a base class.
        /// </summary>
        /// <remarks>
        /// Call this before <see cref="CountPublicConstructors"/>. Checking the constructors first is rejected:
        /// <c>InstanceConstructors</c> builds a new array on every access while <c>AllInterfaces</c> is cached,
        /// so rejecting other types by interface first is about twice as fast.
        /// </remarks>
        public static bool IsConcreteImplementation(INamedTypeSymbol type, INamedTypeSymbol interfaceType)
        {
            return type.TypeKind == TypeKind.Class && !type.IsAbstract && Implements(type, interfaceType);
        }

        /// <summary>
        /// Whether <paramref name="type"/> implements or inherits <paramref name="interfaceType"/>, directly or indirectly.
        /// </summary>
        /// <remarks>
        /// A generic type definition passed as <paramref name="interfaceType"/> matches every construction of it.
        /// </remarks>
        public static bool Implements(ITypeSymbol type, INamedTypeSymbol interfaceType)
        {
            // LINQ Any is rejected for boxing the ImmutableArray enumerator on every call; this runs for every class.
            foreach (var implemented in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(implemented.OriginalDefinition, interfaceType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Counts the public instance constructors, which are the ones the pools can invoke.
        /// </summary>
        /// <param name="type">The pooled type</param>
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

        /// <summary>
        /// Whether <paramref name="symbol"/> has an attribute that the Unity linker treats as <c>[Preserve]</c>.
        /// </summary>
        public static bool HasPreserveAttribute(ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                // Matching UnityEngine.Scripting.PreserveAttribute by symbol is rejected: the Unity linker recognizes
                // any attribute class named PreserveAttribute in any namespace, or deriving from one.
                for (var attributeClass = attribute.AttributeClass;
                     attributeClass != null;
                     attributeClass = attributeClass.BaseType)
                {
                    if (string.Equals(attributeClass.Name, "PreserveAttribute", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
