namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class DefaultArgument
    {
        public void Rent(OperatorPool pool)
        {
            pool.RentAll(default); // TestHelperUI3002
        }
    }
}
