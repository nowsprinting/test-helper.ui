using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class ArgsArrayVariable_AnyConstructorNotPreserved
    {
        public void Register(PaginatorPool pool, object[] args)
        {
            pool.Register<ArgsArrayVariable_AnyConstructorNotPreservedPaginator>(args); // TestHelperUI4008
        }
    }

    public class ArgsArrayVariable_AnyConstructorNotPreservedPaginator : IPaginator
    {
        [Preserve]
        public ArgsArrayVariable_AnyConstructorNotPreservedPaginator(int value) { }
        public ArgsArrayVariable_AnyConstructorNotPreservedPaginator(string text) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
