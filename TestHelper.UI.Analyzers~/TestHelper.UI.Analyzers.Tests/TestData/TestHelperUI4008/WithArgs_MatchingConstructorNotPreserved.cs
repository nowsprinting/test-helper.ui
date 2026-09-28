using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class WithArgs_MatchingConstructorNotPreserved
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<WithArgs_MatchingConstructorNotPreservedPaginator>(1); // TestHelperUI4008
        }
    }

    public class WithArgs_MatchingConstructorNotPreservedPaginator : IPaginator
    {
        public WithArgs_MatchingConstructorNotPreservedPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
