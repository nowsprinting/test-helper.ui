using System.Collections.Generic;
using TestHelper.UI.Operators;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI3002
{
    public class ListArgument
    {
        private readonly List<IOperator> _operators = new List<IOperator>();

        public void Rent(OperatorPool pool)
        {
            pool.RentAll(_operators);
        }
    }
}
