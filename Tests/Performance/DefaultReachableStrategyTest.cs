// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.Strategies;
using Unity.PerformanceTesting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Performance
{
    [TestFixture]
    public class DefaultReachableStrategyTest
    {
        // Not the 1000 buttons of GameObjectFinderTest: a raycast through 1000 stacked buttons grows the heap by
        // about 17 MB, so a collection lands inside most sampling windows and the GC.Alloc median turns negative.
        // 100 buttons keep every sample positive while the nest depth, which drives the per-hit ancestor walk,
        // stays the same.
        private const int ButtonCount = 100;
        private const int NestDepth = 10;

        // Buttons keep the default 100x100 rect at the canvas center, so all but the last four fully overlap
        // ("the stack"). Button_0 is widened to 400 so that fallback raycasts have room beside the stack, and the
        // last four are offset to form a chain of blockers: the pivot hits the last button, then each fallback point
        // (the center of the largest strip left after subtracting the previous blocker) hits the stack and the
        // three remaining offset buttons in turn. That is 5 raycasts, the strategy's maximum, all misses for Button_0.
        private static readonly string s_frontmostButtonName = $"Button_{ButtonCount - 1}";
        private const string BackmostButtonName = "Button_0";
        private static readonly Vector2 s_backmostButtonSize = new Vector2(400f, 100f);

        private static readonly Vector2[] s_lastButtonPositions =
        {
            new Vector2(0f, 45f), // covers the pivot, leaves the bottom strip as the largest remainder
            new Vector2(-80f, 0f),
            new Vector2(-120f, 0f),
            new Vector2(-140f, 0f),
        };

        private const int MaxRaycastCount = 5;

        private static async Task CreateOverlappingButtons()
        {
            UiFixtureFactory.CreateNestedButtons(ButtonCount, NestDepth, withImage: true);

            ((RectTransform)GameObject.Find(BackmostButtonName).transform).sizeDelta = s_backmostButtonSize;
            for (var i = 0; i < s_lastButtonPositions.Length; i++)
            {
                var button = GameObject.Find($"Button_{ButtonCount - 1 - i}");
                ((RectTransform)button.transform).anchoredPosition = s_lastButtonPositions[i];
            }

            // Without a rendered frame, CanvasRenderer.depth stays -1 and GraphicRaycaster skips the buttons.
            Canvas.ForceUpdateCanvases();
            await UniTask.NextFrame();
        }

        private static UniTask MeasureIsReachable(DefaultReachableStrategy strategy, GameObject target)
        {
            return PerformanceMeasurement.MeasureAsync(() =>
            {
                strategy.IsReachable(target, out _);
                return UniTask.CompletedTask;
            });
        }

        [Test]
        [Performance]
        [CreateScene(camera: true)]
        public async Task IsReachable_FrontmostAmongOverlappingButtons_MeasureTimeAndAllocations()
        {
            await CreateOverlappingButtons();
            var target = GameObject.Find(s_frontmostButtonName);
            var strategy = new DefaultReachableStrategy();
            Assume.That(strategy.IsReachable(target, out _), Is.True);

            await MeasureIsReachable(strategy, target);
        }

        [Test]
        [Performance]
        [CreateScene(camera: true)]
        public async Task IsReachable_BackmostAmongOverlappingButtons_MeasureTimeAndAllocations()
        {
            await CreateOverlappingButtons();
            var target = GameObject.Find(BackmostButtonName);
            var strategy = new DefaultReachableStrategy();

            // The verbose logger receives one message per missed raycast, which proves the fixture drives the
            // strategy through its whole fallback loop rather than giving up after the first blocker.
            var spyLogHandler = new SpyLogHandler();
            Assume.That(strategy.IsReachable(target, out _, new Logger(spyLogHandler)), Is.False);
            Assume.That(spyLogHandler.MessageCount, Is.EqualTo(MaxRaycastCount));

            await MeasureIsReachable(strategy, target);
        }

        private class SpyLogHandler : ILogHandler
        {
            public int MessageCount { get; private set; }

            public void LogFormat(LogType logType, Object context, string format, params object[] args)
            {
                MessageCount++;
            }

            public void LogException(Exception exception, Object context)
            {
            }
        }
    }
}
