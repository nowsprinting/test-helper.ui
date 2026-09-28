using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4003
{
    public class NoArgs_ConstructorPreserved
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<NoArgs_ConstructorPreservedOperator>();
        }
    }

    public class NoArgs_ConstructorPreservedOperator : IOperator
    {
        [Preserve]
        public NoArgs_ConstructorPreservedOperator() { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
