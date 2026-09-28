using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class NoArgs_ImplicitDefaultConstructorAndClassNotPreserved
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NoArgs_ImplicitDefaultConstructorAndClassNotPreservedPaginator>(); // TestHelperUI4008
        }
    }

    public class NoArgs_ImplicitDefaultConstructorAndClassNotPreservedPaginator : IPaginator
    {
        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
