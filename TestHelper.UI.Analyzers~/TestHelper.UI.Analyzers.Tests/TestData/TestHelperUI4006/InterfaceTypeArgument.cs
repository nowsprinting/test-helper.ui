using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4006
{
    public class InterfaceTypeArgument
    {
        public void Register(PaginatorPool pool)
        {
            pool.Register<IPaginator>(); // TestHelperUI4006
        }
    }
}
