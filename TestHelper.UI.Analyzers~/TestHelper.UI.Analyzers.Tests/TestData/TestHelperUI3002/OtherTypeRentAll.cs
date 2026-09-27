namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class OtherTypeRentAll
    {
        public void Rent(OtherPool pool)
        {
            pool.RentAll();
        }
    }

    public class OtherPool
    {
        public void RentAll(object? operators = null) { }
    }
}
