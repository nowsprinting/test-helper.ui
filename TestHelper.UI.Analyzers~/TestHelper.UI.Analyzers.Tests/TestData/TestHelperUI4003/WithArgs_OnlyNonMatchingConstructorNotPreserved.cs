using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Scripting;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4003
{
    public class WithArgs_OnlyNonMatchingConstructorNotPreserved
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<WithArgs_OnlyNonMatchingConstructorNotPreservedOperator>(1);
        }
    }

    public class WithArgs_OnlyNonMatchingConstructorNotPreservedOperator : IOperator
    {
        [Preserve]
        public WithArgs_OnlyNonMatchingConstructorNotPreservedOperator(int value) { }
        public WithArgs_OnlyNonMatchingConstructorNotPreservedOperator(string text) { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
