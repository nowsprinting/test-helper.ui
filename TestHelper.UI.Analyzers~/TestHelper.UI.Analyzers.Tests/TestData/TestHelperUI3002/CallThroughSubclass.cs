namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class CallThroughSubclass
    {
        public void Rent(DerivedOperatorPool pool)
        {
            pool.RentAll(); // TestHelperUI3002
        }
    }

    public class DerivedOperatorPool : OperatorPool
    {
    }
}
