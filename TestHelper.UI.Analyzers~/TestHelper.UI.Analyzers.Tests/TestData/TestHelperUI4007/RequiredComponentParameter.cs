using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4007
{
    public class RequiredComponentParameter
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<RequiredComponentParameterPaginator>(); // TestHelperUI4007
        }
    }

    public class RequiredComponentParameterPaginator : IPaginator
    {
        public RequiredComponentParameterPaginator(ScrollRect scrollRect) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
