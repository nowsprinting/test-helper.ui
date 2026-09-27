using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4006
{
    public class WithArgs
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<WithArgsPaginator>(1); // TestHelperUI4006
        }
    }

    public class WithArgsPaginator : IPaginator
    {
        private WithArgsPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
