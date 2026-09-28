using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4003
{
    public class NoArgs_MultipleConstructorsNotPreserved
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<NoArgs_MultipleConstructorsNotPreservedOperator>();
        }
    }

    public class NoArgs_MultipleConstructorsNotPreservedOperator : IOperator
    {
        public NoArgs_MultipleConstructorsNotPreservedOperator() { }
        public NoArgs_MultipleConstructorsNotPreservedOperator(int value) { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
