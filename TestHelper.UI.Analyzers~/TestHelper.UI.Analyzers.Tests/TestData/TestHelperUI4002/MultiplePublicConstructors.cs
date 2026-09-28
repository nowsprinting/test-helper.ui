using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4002
{
    public class MultiplePublicConstructors
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<MultiplePublicConstructorsOperator>();
        }
    }

    public class MultiplePublicConstructorsOperator : IOperator
    {
        public MultiplePublicConstructorsOperator(int value) { }
        public MultiplePublicConstructorsOperator(string text) { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
