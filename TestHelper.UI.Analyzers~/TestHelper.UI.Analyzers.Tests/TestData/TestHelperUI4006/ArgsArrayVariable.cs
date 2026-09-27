using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4006
{
    public class ArgsArrayVariable
    {
        public void Register(PaginatorPool pool, object[] args)
        {
            pool.Register<ArgsArrayVariablePaginator>(args); // TestHelperUI4006
        }
    }

    public class ArgsArrayVariablePaginator : IPaginator
    {
        private ArgsArrayVariablePaginator() { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
