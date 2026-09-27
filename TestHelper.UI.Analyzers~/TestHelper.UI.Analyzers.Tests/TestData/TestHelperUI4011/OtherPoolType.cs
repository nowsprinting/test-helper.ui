using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Paginators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4011
{
    public class OtherPoolType
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<OtherPoolTypePaginator>("s");
        }
    }

    public class OtherPoolTypePaginator : IPaginator
    {
        public OtherPoolTypePaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
