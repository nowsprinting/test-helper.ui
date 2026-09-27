using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4012
{
    public class GenericTypeParameter
    {
        public void Register<T>(PaginatorPool pool) where T : class, IPaginator
        {
            pool.Register<T>("s");
        }
    }
}
