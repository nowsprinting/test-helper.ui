using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4001
{
    public class InterfaceTypeArgument
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<IClickOperator>(); // TestHelperUI4001
        }
    }
}
