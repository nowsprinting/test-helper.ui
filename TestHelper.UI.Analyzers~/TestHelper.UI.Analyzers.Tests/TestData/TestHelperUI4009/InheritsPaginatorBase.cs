using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4009
{
    public abstract class PaginatorBase : IPaginator<MonoBehaviour>
    {
        public MonoBehaviour? TargetComponent { get; set; }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }

    public class InheritsPaginatorBase : PaginatorBase // TestHelperUI4009
    {
        public InheritsPaginatorBase()
        {
        }

        public InheritsPaginatorBase(int value)
        {
        }
    }
}
