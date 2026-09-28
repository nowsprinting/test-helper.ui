using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class NoArgs_ConstructorPreserved
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NoArgs_ConstructorPreservedPaginator>();
        }
    }

    public class NoArgs_ConstructorPreservedPaginator : IPaginator
    {
        [Preserve]
        public NoArgs_ConstructorPreservedPaginator() { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
