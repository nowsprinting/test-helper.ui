using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4009
{
    public class PartialWithConstructorsInEachPart
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<PartialWithConstructorsInEachPartPaginator>(); // TestHelperUI4009
        }
    }

    public partial class PartialWithConstructorsInEachPartPaginator : IPaginator
    {
        public PartialWithConstructorsInEachPartPaginator() { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }

    public partial class PartialWithConstructorsInEachPartPaginator
    {
        public PartialWithConstructorsInEachPartPaginator(int value)
        {
        }
    }
}
