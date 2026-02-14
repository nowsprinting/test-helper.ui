// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.TestDoubles
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class FakeOperator : IOperator
    {
        public int IntValue { get; set; }
        public ILogger Logger { get; set; }
        public ScreenshotOptions ScreenshotOptions { get; set; }
        public IVisualizer Visualizer { get; set; }

        public FakeOperator(int intValue = 0, ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            IVisualizer visualizer = null)
        {
            IntValue = intValue;
            Logger = logger;
            ScreenshotOptions = screenshotOptions;
            Visualizer = visualizer;
        }

        public bool CanOperate(GameObject gameObject)
        {
            throw new NotImplementedException();
        }

        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
