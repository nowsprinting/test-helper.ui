namespace TestHelper.UI.Analyzers.Utilities
{
    /// <summary>
    /// The problems of a <c>Register&lt;T&gt;(params object[] args)</c> call on <c>OperatorPool</c> or <c>PaginatorPool</c>,
    /// in descending priority. A call is reported for only its highest-priority problem.
    /// </summary>
    internal enum PoolRegistrationRule
    {
        None,
        NoPublicConstructor,
        MultiplePublicConstructors,
        RequiredParameter,
        NoMatchingConstructor,
        NotPreserved
    }
}
