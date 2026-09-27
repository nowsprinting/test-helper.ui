namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class NamedNullArgument
    {
        public void Rent(OperatorPool pool)
        {
            pool.RentAll(operators: null); // TestHelperUI3002
        }
    }
}
