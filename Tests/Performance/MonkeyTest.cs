// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.Random;
using TestHelper.UI.Operators;
using Unity.PerformanceTesting;
using UnityEngine;

namespace TestHelper.UI.Performance
{
    [TestFixture]
    public class MonkeyTest
    {
        private const int GridButtonCount = 10;

        [Test]
        [Performance]
        [CreateScene(camera: true)]
        public async Task RunStep_SeededRandom_MeasureTimeAndAllocations()
        {
            UiFixtureFactory.CreateGridButtons(GridButtonCount);
            // Without a rendered frame, CanvasRenderer.depth stays -1 and GraphicRaycaster skips the buttons.
            Canvas.ForceUpdateCanvases();
            await UniTask.NextFrame();

            var config = new MonkeyConfig
            {
                Random = new RandomWrapper(0), // pin seed for branch comparison
                // Only the click operator is registered; UguiClickAndHoldOperator's hold time would dominate the
                // measurement, hiding the cost of the lottery and reachability paths this test is meant to sample.
                OperatorPool = new OperatorPool().Register<UguiClickOperator>()
            };
            var finder = new InteractableComponentsFinder(config.IsInteractable, config.OperatorPool);

            var (didAction, _) = await Monkey.RunStep(
                config.Random, config.Logger, finder, config.IgnoreStrategy, config.ReachableStrategy);
            Assume.That(didAction, Is.True); // the generated buttons are actually operable

            await PerformanceMeasurement.MeasureAsync(() => Monkey.RunStep(
                config.Random, config.Logger, finder, config.IgnoreStrategy, config.ReachableStrategy));
        }
    }
}
