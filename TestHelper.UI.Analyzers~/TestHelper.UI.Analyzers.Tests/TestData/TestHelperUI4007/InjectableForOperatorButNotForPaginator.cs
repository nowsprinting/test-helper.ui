using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4007
{
    public class InjectableForOperatorButNotForPaginator
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<InjectableForOperatorButNotForPaginatorPaginator>(); // TestHelperUI4007
        }
    }

    public class InjectableForOperatorButNotForPaginatorPaginator : IPaginator
    {
        public InjectableForOperatorButNotForPaginatorPaginator(ILogger logger) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
