using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    public interface IOperator
    {
        ILogger Logger { set; }

        ScreenshotOptions ScreenshotOptions { set; }

        IVisualizer Visualizer { set; }

        bool CanOperate(GameObject gameObject);

        UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);
    }
}
