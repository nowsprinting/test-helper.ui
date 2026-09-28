using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class NoArgs_RequiredParameterAndNotPreserved
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NoArgs_RequiredParameterAndNotPreservedPaginator>();
        }
    }

    public class NoArgs_RequiredParameterAndNotPreservedPaginator : IPaginator
    {
        public NoArgs_RequiredParameterAndNotPreservedPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
