using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4001
{
    public class ImplementsClickOperatorWithPrivateConstructor : IClickOperator   // TestHelperUI4001
    {
        private ImplementsClickOperatorWithPrivateConstructor() { }

        public ILogger? Logger { get; set; }
        public ScreenshotOptions? ScreenshotOptions { get; set; }
        public IVisualizer? Visualizer { get; set; }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
