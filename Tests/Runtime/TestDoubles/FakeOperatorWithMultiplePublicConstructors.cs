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
using UnityEngine.Scripting;

namespace TestHelper.UI.TestDoubles
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    // Keeping only one public constructor is rejected: this fake verifies that OperatorPool
    // rejects renting an IOperator type with multiple public constructors.
    public class FakeOperatorWithMultiplePublicConstructors : IOperator
    {
        public int IntValue { get; }

        public ILogger Logger { get; set; }
        public ScreenshotOptions ScreenshotOptions { get; set; }
        public IVisualizer Visualizer { get; set; }

        [Preserve]
        public FakeOperatorWithMultiplePublicConstructors() { }

        [Preserve]
        public FakeOperatorWithMultiplePublicConstructors(int intValue)
        {
            IntValue = intValue;
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
