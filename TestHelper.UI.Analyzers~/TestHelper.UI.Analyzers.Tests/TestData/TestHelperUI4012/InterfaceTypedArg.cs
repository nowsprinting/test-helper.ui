using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class InterfaceTypedArg
    {
        public void Register(PaginatorPool pool, IComparable arg)
        {
            pool.Register<InterfaceTypedArgPaginator>(arg);
        }
    }

    public class InterfaceTypedArgPaginator : IPaginator
    {
        public InterfaceTypedArgPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
