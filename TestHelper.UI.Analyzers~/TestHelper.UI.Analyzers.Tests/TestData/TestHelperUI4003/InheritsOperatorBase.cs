using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4003
{
    public abstract class OperatorBaseWithoutPreserve : IClickOperator
    {
        public ILogger? Logger { get; set; }
        public ScreenshotOptions? ScreenshotOptions { get; set; }
        public IVisualizer? Visualizer { get; set; }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }

    public class InheritsOperatorBase : OperatorBaseWithoutPreserve
    {
        public InheritsOperatorBase(int value = 0) // TestHelperUI4003
        {
        }
    }
}
