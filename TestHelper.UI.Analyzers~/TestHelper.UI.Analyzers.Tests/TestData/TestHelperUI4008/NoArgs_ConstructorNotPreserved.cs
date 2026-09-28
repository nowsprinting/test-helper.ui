using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class NoArgs_ConstructorNotPreserved
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NoArgs_ConstructorNotPreservedPaginator>(); // TestHelperUI4008
        }
    }

    public class NoArgs_ConstructorNotPreservedPaginator : IPaginator
    {
        public NoArgs_ConstructorNotPreservedPaginator() { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
