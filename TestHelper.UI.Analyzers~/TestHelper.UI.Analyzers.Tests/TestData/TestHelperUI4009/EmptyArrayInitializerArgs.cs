using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4009
{
    public class EmptyArrayInitializerArgs
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<EmptyArrayInitializerArgsPaginator>(new object[] { }); // TestHelperUI4009
        }
    }

    public class EmptyArrayInitializerArgsPaginator : IPaginator
    {
        public EmptyArrayInitializerArgsPaginator() { }
        public EmptyArrayInitializerArgsPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
