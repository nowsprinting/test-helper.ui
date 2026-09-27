using System.Collections.Generic;
using TestHelper.UI.Operators;

namespace TestHelper.UI
{
    public class OperatorPool
    {
        // The real signature is nullable-oblivious; annotating it lets fixtures pass null without CS8625.
        public OperatorPool Register<T>(params object?[]? args) where T : class, IOperator =>
            throw new System.NotImplementedException();

        public IReadOnlyList<IOperator> RentAll(List<IOperator>? operators = null) =>
            throw new System.NotImplementedException();

        public T Rent<T>() where T : class, IOperator => throw new System.NotImplementedException();
    }
}
