namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class ConditionalAccess
    {
        public void Rent(OperatorPool? pool)
        {
            pool?.RentAll(); // TestHelperUI3002
        }
    }
}
