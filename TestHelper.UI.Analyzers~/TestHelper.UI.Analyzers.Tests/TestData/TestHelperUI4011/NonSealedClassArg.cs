using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4011
{
    public class NonSealedClassArg
    {
        public void Register(OperatorPool pool, NonSealedClassArgBase arg)
        {
            pool.Register<NonSealedClassArgOperator>(arg);
        }
    }

    public class NonSealedClassArgOperator : IOperator
    {
        public NonSealedClassArgOperator(NonSealedClassArgDerived derived) { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }

    public class NonSealedClassArgBase
    {
    }

    public class NonSealedClassArgDerived : NonSealedClassArgBase
    {
    }
}
