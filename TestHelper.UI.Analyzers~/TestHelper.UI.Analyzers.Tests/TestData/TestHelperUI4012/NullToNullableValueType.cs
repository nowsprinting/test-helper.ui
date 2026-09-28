using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class NullToNullableValueType
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<NullToNullableValueTypePaginator>(null, 1);
        }
    }

    public class NullToNullableValueTypePaginator : IPaginator
    {
        public NullToNullableValueTypePaginator(int? first, int second) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
