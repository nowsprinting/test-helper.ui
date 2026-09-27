using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class FewerArgsThanParameters
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<FewerArgsThanParametersPaginator>(1);
        }
    }

    public class FewerArgsThanParametersPaginator : IPaginator
    {
        public FewerArgsThanParametersPaginator(int value, string? text = null) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
