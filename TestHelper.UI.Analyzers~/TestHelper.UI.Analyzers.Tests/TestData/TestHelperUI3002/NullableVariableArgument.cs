using System.Collections.Generic;
using TestHelper.UI.Operators;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class NullableVariableArgument
    {
        public void Rent(OperatorPool pool, List<IOperator>? operators)
        {
            pool.RentAll(operators);
        }
    }
}
