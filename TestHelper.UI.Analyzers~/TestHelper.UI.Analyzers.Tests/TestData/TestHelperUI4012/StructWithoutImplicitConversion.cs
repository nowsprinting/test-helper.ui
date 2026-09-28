using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class StructWithoutImplicitConversion
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<StructWithoutImplicitConversionPaginator>(1.5); // TestHelperUI4012
        }
    }

    public class StructWithoutImplicitConversionPaginator : IPaginator
    {
        public StructWithoutImplicitConversionPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
