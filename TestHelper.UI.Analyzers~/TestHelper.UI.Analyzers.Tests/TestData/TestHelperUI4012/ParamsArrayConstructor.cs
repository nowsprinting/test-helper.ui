using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class ParamsArrayConstructor
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<ParamsArrayConstructorPaginator>(1, 2, 3);
        }
    }

    public class ParamsArrayConstructorPaginator : IPaginator
    {
        public ParamsArrayConstructorPaginator(params int[] values) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
