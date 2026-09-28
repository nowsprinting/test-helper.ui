using System;
using System.Collections.Generic;
using TestHelper.Random;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using TestHelper.UI.Visualizers;
using UnityEngine;

namespace TestHelper.UI
{
    public class OperatorPool
    {
#nullable disable
        public OperatorPool(
            ILogger logger = null,
            ScreenshotOptions screenshotOptions = null,
            IVisualizer visualizer = null,
            Func<GameObject, Vector2> getScreenPoint = null,
            IReachableStrategy reachableStrategy = null,
            IRandom random = null,
            bool requireRegistration = true) =>
            throw new NotImplementedException();

        public OperatorPool Register<T>(params object[] args) where T : class, IOperator =>
            throw new NotImplementedException();
#nullable restore

        public IReadOnlyList<IOperator> RentAll(List<IOperator>? operators = null) =>
            throw new NotImplementedException();

        public T Rent<T>() where T : class, IOperator => throw new NotImplementedException();
    }
}
