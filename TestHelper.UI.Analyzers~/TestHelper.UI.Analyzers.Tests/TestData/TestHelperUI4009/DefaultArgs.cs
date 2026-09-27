using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4009
{
    public class DefaultArgs
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<DefaultArgsPaginator>(default); // TestHelperUI4009
        }
    }

    public class DefaultArgsPaginator : IPaginator
    {
        public DefaultArgsPaginator() { }
        public DefaultArgsPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
