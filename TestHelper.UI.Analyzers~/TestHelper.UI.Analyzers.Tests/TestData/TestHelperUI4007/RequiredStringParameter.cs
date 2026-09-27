using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4007
{
    public class RequiredStringParameter
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<RequiredStringParameterPaginator>(); // TestHelperUI4007
        }
    }

    public class RequiredStringParameterPaginator : IPaginator
    {
        public RequiredStringParameterPaginator(string text) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
