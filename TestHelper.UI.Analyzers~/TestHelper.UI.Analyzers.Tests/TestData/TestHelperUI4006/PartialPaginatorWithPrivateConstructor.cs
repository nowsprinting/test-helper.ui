using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4006
{
    public partial class PartialPaginatorWithPrivateConstructor : IPaginator<MonoBehaviour> // TestHelperUI4006
    {
        private PartialPaginatorWithPrivateConstructor() { }
    }

    public partial class PartialPaginatorWithPrivateConstructor
    {
        public MonoBehaviour? TargetComponent { get; set; }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
