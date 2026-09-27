using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class NoConstructorMatchesAmongMultiple
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NoConstructorMatchesAmongMultiplePaginator>("s"); // TestHelperUI4012
        }
    }

    public class NoConstructorMatchesAmongMultiplePaginator : IPaginator
    {
        public NoConstructorMatchesAmongMultiplePaginator(int value) { }
        public NoConstructorMatchesAmongMultiplePaginator(bool value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
