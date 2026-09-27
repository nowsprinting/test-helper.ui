namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class NullArgument
    {
        public void Rent(OperatorPool pool)
        {
            pool.RentAll(null); // TestHelperUI3002
        }
    }
}
