using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4007
{
    public class AllParametersHaveDefaultValues
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<AllParametersHaveDefaultValuesPaginator>();
        }
    }

    public class AllParametersHaveDefaultValuesPaginator : IPaginator
    {
        public AllParametersHaveDefaultValuesPaginator(int value = 1, string? text = null) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
