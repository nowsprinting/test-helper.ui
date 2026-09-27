using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI
{
    public class PaginatorPool
    {
        // The real signature is nullable-oblivious; annotating it lets fixtures pass null without CS8625.
        public PaginatorPool Register<T>(params object?[]? args) where T : class, IPaginator =>
            throw new System.NotImplementedException();

        public T Rent<T>(MonoBehaviour? targetComponent = null) where T : class, IPaginator =>
            throw new System.NotImplementedException();
    }
}
