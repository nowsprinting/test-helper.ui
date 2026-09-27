using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI
{
    public class PaginatorPool
    {
        public PaginatorPool(bool requireRegistration = true) => throw new System.NotImplementedException();

#nullable disable
        public PaginatorPool Register<T>(params object[] args) where T : class, IPaginator =>
            throw new System.NotImplementedException();

        public T Rent<T>(MonoBehaviour targetComponent = null) where T : class, IPaginator =>
            throw new System.NotImplementedException();
#nullable restore
    }
}
