namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class DefaultArgument
    {
        public void Rent(OperatorPool pool)
        {
            // Omitting the redundant argument is rejected: the explicit constant null is the subject of this case.
            // ReSharper disable once RedundantArgumentDefaultValue
            pool.RentAll(default); // TestHelperUI3002
        }
    }
}
