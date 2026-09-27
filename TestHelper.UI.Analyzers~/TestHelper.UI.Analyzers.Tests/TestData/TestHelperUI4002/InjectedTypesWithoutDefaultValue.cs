using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Random;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Analyzers.Tests.TestData.TestHelperUI4002
{
    public class InjectedTypesWithoutDefaultValue
    {
        public void Register(OperatorPool pool)
        {
            pool.Register<InjectedTypesWithoutDefaultValueOperator>();
        }
    }

    public class InjectedTypesWithoutDefaultValueOperator : IOperator
    {
        public InjectedTypesWithoutDefaultValueOperator(ILogger logger, ScreenshotOptions screenshotOptions, IVisualizer visualizer,
            Func<GameObject, Vector2> getScreenPoint, IReachableStrategy reachableStrategy, IRandom random) { }

        public ILogger Logger { set { } }
        public ScreenshotOptions ScreenshotOptions { set { } }
        public IVisualizer Visualizer { set { } }

        public bool CanOperate(GameObject gameObject) => throw new System.NotImplementedException();

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }
}
