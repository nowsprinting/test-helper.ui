using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class ComponentParameter
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<ComponentParameterPaginator>(1); // TestHelperUI4012
        }
    }

    public class ComponentParameterPaginator : IPaginator
    {
        public ComponentParameterPaginator(ScrollRect? scrollRect = null) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
