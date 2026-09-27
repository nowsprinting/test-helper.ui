using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4006
{
    public abstract class PaginatorBase : IPaginator<MonoBehaviour>
    {
        protected PaginatorBase() { }

        public MonoBehaviour? TargetComponent { get; set; }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }

    public class InheritsPaginatorBaseWithPrivateConstructor : PaginatorBase // TestHelperUI4006
    {
        private InheritsPaginatorBaseWithPrivateConstructor() { }
    }
}
