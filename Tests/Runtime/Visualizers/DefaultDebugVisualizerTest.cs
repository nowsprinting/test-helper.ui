// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Visualizers
{
    [TestFixture]
    public class DefaultDebugVisualizerTest
    {
        private const string TestScenePath = "../../Scenes/Canvas.unity";
        private const float IndicatorLifetime = 0.2f;
        private readonly GameObjectFinder _finder = new GameObjectFinder(0.1d);
        private readonly IVisualizer _sut = new DefaultDebugVisualizer() { IndicatorLifetime = IndicatorLifetime };
        private readonly List<GameObject> _referenceObjects = new List<GameObject>();

        [SetUp]
        public async Task SetUp()
        {
            var canvas = await _finder.FindByNameAsync("Canvas", reachable: false);
            // Note: CanvasScaler settings: Scale With Screen Size, Reference Resolution: 640x480, Match Width Or Height: 0

            // Create reference images and screen points
            _referenceObjects.Clear();
            var anchoredPositions = new Vector2[]
            {
                new Vector2(-200, -100),
                new Vector2(-200, 100),
                new Vector2(200, -100),
                new Vector2(200, 100)
            };
            foreach (var anchoredPosition in anchoredPositions)
            {
                var image = new GameObject(null, typeof(Image));
                image.transform.SetParent(canvas.GameObject.transform, false);
                image.GetComponent<Image>().color = Color.gray;
                image.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
                image.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 30); // same as a Button's default
                _referenceObjects.Add(image);
            }
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowNotReachableIndicator_Horizontal([Values] GameViewResolution resolution)
        {
            var (width, height, name) = resolution.GetParameter();
            GameViewControlHelper.SetResolution(width, height, name);
            await UniTask.NextFrame();

            foreach (var reference in _referenceObjects)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, reference.transform.position);
                _sut.ShowNotReachableIndicator(screenPoint, reference);
            }

            await ScreenshotHelper.TakeScreenshotAsync();
            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowNotReachableIndicator_Vertical([Values] GameViewResolution resolution)
        {
            var (width, height, name) = resolution.GetParameter();
            GameViewControlHelper.SetResolution(height, width, name); // flip
            await UniTask.NextFrame();

            foreach (var reference in _referenceObjects)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, reference.transform.position);
                _sut.ShowNotReachableIndicator(screenPoint, reference);
            }

            await ScreenshotHelper.TakeScreenshotAsync();
            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime)); // wait for end of life
        }
    }
}
