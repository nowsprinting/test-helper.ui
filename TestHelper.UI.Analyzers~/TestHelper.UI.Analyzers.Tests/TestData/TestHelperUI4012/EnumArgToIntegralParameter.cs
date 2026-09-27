using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class EnumArgToIntegralParameter
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<EnumArgToIntegralParameterPaginator>(EnumArgToIntegralParameterKind.First);
        }
    }

    public class EnumArgToIntegralParameterPaginator : IPaginator
    {
        public EnumArgToIntegralParameterPaginator(int value) { }

        public MonoBehaviour TargetComponent { set { } }

        public UniTask ResetAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default) =>
            throw new System.NotImplementedException();

        public bool HasNextPage() => throw new System.NotImplementedException();
    }

    public enum EnumArgToIntegralParameterKind
    {
        First,
    }
}
