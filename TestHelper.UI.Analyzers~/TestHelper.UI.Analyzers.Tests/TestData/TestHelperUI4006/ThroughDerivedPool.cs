using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4006
{
    public class ThroughDerivedPool
    {
        public void Register(ThroughDerivedPoolPool pool)
        {
            pool.Register<ThroughDerivedPoolPaginator>(); // TestHelperUI4006
        }
    }

    public class ThroughDerivedPoolPaginator : IPaginator
    {
        private ThroughDerivedPoolPaginator() { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }

    public class ThroughDerivedPoolPool : PaginatorPool
    {
    }
}
