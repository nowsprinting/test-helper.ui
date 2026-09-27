using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TestHelper.UI.Analyzers.Utilities
{
    /// <summary>
    /// Classifies <c>Register&lt;T&gt;</c> calls on <c>OperatorPool</c> and <c>PaginatorPool</c> by the problem
    /// the pool hits when it creates an instance of <c>T</c> via reflection.
    /// </summary>
    internal static class PoolRegistration
    {
        public const string OperatorPoolMetadataName = "TestHelper.UI.OperatorPool";
        public const string PaginatorPoolMetadataName = "TestHelper.UI.PaginatorPool";

        /// <summary>
        /// Registers an action that reports <paramref name="descriptor"/> at each <c>Register&lt;T&gt;</c> call
        /// on the pool whose highest-priority problem is <paramref name="rule"/>.
        /// </summary>
        /// <param name="context">The analyzer's context</param>
        /// <param name="poolMetadataName">The metadata name of the pool type</param>
        /// <param name="injectsParameters">Whether the pool injects its own values into constructor parameters (<c>OperatorPool</c> does)</param>
        /// <param name="rule">The rule to report</param>
        /// <param name="descriptor">The descriptor to report</param>
        public static void RegisterRuleAction(AnalysisContext context, string poolMetadataName, bool injectsParameters,
            PoolRegistrationRule rule, DiagnosticDescriptor descriptor)
        {
        }
    }
}
