using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class NullToNonNullableValueType
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NullToNonNullableValueTypePaginator>(null, 1); // TestHelperUI4012
        }
    }

    public class NullToNonNullableValueTypePaginator : IPaginator
    {
        public NullToNonNullableValueTypePaginator(int first, int second) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
