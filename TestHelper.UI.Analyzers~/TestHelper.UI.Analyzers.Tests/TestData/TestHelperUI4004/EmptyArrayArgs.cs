using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4004
{
    public class EmptyArrayArgs
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<EmptyArrayArgsOperator>(new object[0]); // TestHelperUI4004
        }
    }

    public class EmptyArrayArgsOperator : IOperator
    {
        public EmptyArrayArgsOperator() { }
        public EmptyArrayArgsOperator(int value) { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
