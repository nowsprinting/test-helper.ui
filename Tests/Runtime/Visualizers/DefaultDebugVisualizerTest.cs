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
        private readonly List<GameObject> _referenceObjects = new List<GameObject>();
        private DefaultDebugVisualizer _sut;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _sut = new DefaultDebugVisualizer() { IndicatorLifetime = IndicatorLifetime };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _sut.Dispose();
        }

        [SetUp]
        public async Task SetUp()
        {
            var finder = new GameObjectFinder(0.1d);
            var canvas = await finder.FindByNameAsync("Canvas", reachable: false);
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

            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowNotReachableIndicator_Vertical([Values] GameViewResolution resolution)
        {
            var (width, height, name) = resolution.GetParameter();
            GameViewControlHelper.SetResolution(height, width, $"{name} Portrait");
            await UniTask.NextFrame();

            foreach (var reference in _referenceObjects)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, reference.transform.position);
                _sut.ShowNotReachableIndicator(screenPoint, reference);
            }

            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        public void ShowNotReachableIndicator_IndicatorIsShown()
        {
            var screenPoint = new Vector2(100, 100);

            _sut.ShowNotReachableIndicator(screenPoint);

            var indicator = GameObject.Find("Indicator");
            Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("eye_slash"));
            Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public void ShowNotReachableIndicator_WithBlocker_BlockerIndicatorIsShown()
        {
            var blocker = _referenceObjects[0];
            var screenPoint = new Vector2(100, 100);

            _sut.ShowNotReachableIndicator(screenPoint, blocker);

            var indicator = GameObject.Find("Blocker Indicator");
            Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowNotReachableIndicator_AfterLifetime_IndicatorIsDeactivated()
        {
            var screenPoint = new Vector2(100, 100);

            _sut.ShowNotReachableIndicator(screenPoint);

            var indicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime + 0.1));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowNotReachableIndicator_CalledAfterReturn_IndicatorIsReused()
        {
            var screenPoint = new Vector2(100, 100);

            _sut.ShowNotReachableIndicator(screenPoint);

            var firstIndicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime + 0.1));
            _sut.ShowNotReachableIndicator(screenPoint);
            var secondIndicator = GameObject.Find("Indicator");
            Assert.That(secondIndicator, Is.SameAs(firstIndicator));
        }

        [Test]
        [LoadScene(TestScenePath)]
        public void ShowNotInteractableIndicator_IndicatorIsShown()
        {
            var target = _referenceObjects[0];

            _sut.ShowNotInteractableIndicator(target);

            var indicator = GameObject.Find("Indicator");
            Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("hand_slash"));
            Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowNotInteractableIndicator_AfterLifetime_IndicatorIsDeactivated()
        {
            var target = _referenceObjects[0];

            _sut.ShowNotInteractableIndicator(target);

            var indicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime + 0.1));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public void ShowIgnoredIndicator_IndicatorIsShown()
        {
            var target = _referenceObjects[0];

            _sut.ShowIgnoredIndicator(target);

            var indicator = GameObject.Find("Indicator");
            Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("lock"));
            Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public async Task ShowIgnoredIndicator_AfterLifetime_IndicatorIsDeactivated()
        {
            var target = _referenceObjects[0];

            _sut.ShowIgnoredIndicator(target);

            var indicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(IndicatorLifetime + 0.1));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }
    }
}
