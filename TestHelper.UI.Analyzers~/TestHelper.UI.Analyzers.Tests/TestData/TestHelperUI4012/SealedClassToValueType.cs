using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class SealedClassToValueType
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<SealedClassToValueTypePaginator>("s"); // TestHelperUI4012
        }
    }

    public class SealedClassToValueTypePaginator : IPaginator
    {
        public SealedClassToValueTypePaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
