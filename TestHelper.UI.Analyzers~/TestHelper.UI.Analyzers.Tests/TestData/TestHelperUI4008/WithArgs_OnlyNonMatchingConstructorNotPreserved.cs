using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4008
{
    public class WithArgs_OnlyNonMatchingConstructorNotPreserved
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<WithArgs_OnlyNonMatchingConstructorNotPreservedPaginator>(1);
        }
    }

    public class WithArgs_OnlyNonMatchingConstructorNotPreservedPaginator : IPaginator
    {
        [Preserve]
        public WithArgs_OnlyNonMatchingConstructorNotPreservedPaginator(int value) { }
        public WithArgs_OnlyNonMatchingConstructorNotPreservedPaginator(string text) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }
}
