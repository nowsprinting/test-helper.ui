// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.RuntimeInternals;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace TestHelper.UI.Visualizers
{
    [TestFixture]
    public class DefaultDebugVisualizerTest
    {
        private const string TestScenePath = "../../Scenes/Canvas.unity";
        private const float TestTimeScale = 5.0f; // Speed up time to shorten the test duration
        private readonly List<GameObject> _referenceObjects = new List<GameObject>();
        private DefaultDebugVisualizer _sut;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _sut = new DefaultDebugVisualizer();
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
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_Horizontal(
            [Values(GameViewResolution.FullHD, GameViewResolution.WQVGA, GameViewResolution.VGA)]
            GameViewResolution resolution)
        {
            var (width, height, name) = resolution.GetParameter();
            GameViewControlHelper.SetResolution(width, height, name);
            await UniTask.NextFrame();

            foreach (var reference in _referenceObjects)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, reference.transform.position);
                _sut.ShowNotReachableIndicator(screenPoint, reference);
            }

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_Vertical(
            [Values(GameViewResolution.FullHD, GameViewResolution.WQVGA, GameViewResolution.VGA)]
            GameViewResolution resolution)
        {
            var (width, height, name) = resolution.GetParameter();
            GameViewControlHelper.SetResolution(height, width, $"{name} Portrait");
            await UniTask.NextFrame();

            foreach (var reference in _referenceObjects)
            {
                var screenPoint = RectTransformUtility.WorldToScreenPoint(null, reference.transform.position);
                _sut.ShowNotReachableIndicator(screenPoint, reference);
            }

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_IndicatorIsShown()
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, _referenceObjects[0].transform.position);
            _sut.ShowNotReachableIndicator(screenPoint);

            var indicator = GameObject.Find("Indicator");
            var image = indicator.GetComponent<Image>();
            Assert.That(image.sprite.name, Is.EqualTo("eye_slash"));
            Assert.That(image.raycastTarget, Is.False);

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_WithBlocker_BlockerIndicatorIsShown()
        {
            var blocker = _referenceObjects[0];
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, blocker.transform.position);
            _sut.ShowNotReachableIndicator(screenPoint, blocker);

            var indicator = GameObject.Find("Blocker Indicator");
            var image = indicator.GetComponent<Image>();
            Assert.That(image.raycastTarget, Is.False);

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_AfterLifetime_IndicatorIsDeactivated()
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, _referenceObjects[0].transform.position);
            _sut.ShowNotReachableIndicator(screenPoint);

            var indicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.1f));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_AfterLifetime_BlockIndicatorIsDeactivated()
        {
            var blocker = _referenceObjects[0];
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, blocker.transform.position);
            _sut.ShowNotReachableIndicator(screenPoint, blocker);

            var indicator = GameObject.Find("Blocker Indicator");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.1f));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_CalledAfterReturn_IndicatorIsReused()
        {
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, _referenceObjects[0].transform.position);
            _sut.ShowNotReachableIndicator(screenPoint);

            var firstIndicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.1f));

            _sut.ShowNotReachableIndicator(screenPoint);
            var secondIndicator = GameObject.Find("Indicator");
            Assert.That(secondIndicator, Is.SameAs(firstIndicator)); // latest indicator is reused

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotReachableIndicator_CalledAfterReturn_BlockerIndicatorIsReused()
        {
            var blocker = _referenceObjects[0];
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, blocker.transform.position);
            _sut.ShowNotReachableIndicator(screenPoint, blocker);

            var firstIndicator = GameObject.Find("Blocker Indicator");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.1f));

            _sut.ShowNotReachableIndicator(screenPoint, blocker);
            var secondIndicator = GameObject.Find("Blocker Indicator");
            Assert.That(secondIndicator, Is.SameAs(firstIndicator)); // latest indicator is reused

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotInteractableIndicator_IndicatorIsShown()
        {
            var target = _referenceObjects[0];
            _sut.ShowNotInteractableIndicator(target);

            var indicator = GameObject.Find("Indicator");
            Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("hand_slash"));
            Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowNotInteractableIndicator_AfterLifetime_IndicatorIsDeactivated()
        {
            var target = _referenceObjects[0];
            _sut.ShowNotInteractableIndicator(target);

            var indicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.1f));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowIgnoredIndicator_IndicatorIsShown()
        {
            var target = _referenceObjects[0];
            _sut.ShowIgnoredIndicator(target);

            var indicator = GameObject.Find("Indicator");
            Assert.That(indicator.GetComponent<Image>().sprite.name, Is.EqualTo("lock"));
            Assert.That(indicator.GetComponent<Image>().raycastTarget, Is.False);

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowIgnoredIndicator_AfterLifetime_IndicatorIsDeactivated()
        {
            var target = _referenceObjects[0];
            _sut.ShowIgnoredIndicator(target);

            var indicator = GameObject.Find("Indicator");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.1f));
            Assert.That(indicator.activeInHierarchy, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowPointerOperationEffect_IndicatorIsShown(
            [Values(GameViewResolution.FullHD, GameViewResolution.WQVGA, GameViewResolution.VGA)]
            GameViewResolution resolution)
        {
            var (width, height, name) = resolution.GetParameter();
            GameViewControlHelper.SetResolution(width, height, name);
            await UniTask.NextFrame();

            _sut.ShowPointerOperationEffect(_referenceObjects[0]);

            var ripple = GameObject.Find("Ripple");
            Assert.That(ripple.GetComponent<Image>().sprite.name, Is.EqualTo("ripple"));
            Assert.That(ripple.GetComponent<Image>().raycastTarget, Is.False);

            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.2f)); // wait for end of life
        }

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowPointerOperationEffect_AfterLifetime_IndicatorIsDeactivated()
        {
            _sut.ShowPointerOperationEffect(_referenceObjects[0]);

            var ripple = GameObject.Find("Ripple");
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.2f)); // wait for end of life

            Assert.That(ripple.activeInHierarchy, Is.False);
        }

#if UNITY_2021_1_OR_NEWER
        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        [Category("IgnoreCI")]
        public async Task ShowPointerOperationEffect_CalledAfterReturn_IndicatorIsReused()
        {
            _sut.ShowPointerOperationEffect(_referenceObjects[0]);
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.2f)); // wait for end of life

            _sut.ShowPointerOperationEffect(_referenceObjects[1]);
            await Task.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime + 0.2f)); // wait for end of life

            var rippleCount = GameObject.FindObjectsByType<SpreadBehaviour>(FindObjectsInactive.Include,
                FindObjectsSortMode.None).Length;
            Assert.That(rippleCount, Is.EqualTo(3)); // reused (not 6)
        }
#endif

        [Test]
        [LoadScene(TestScenePath)]
        [TimeScale(TestTimeScale)]
        public async Task ShowPointerOperationEffect_Disposed_ThrowsInvalidOperationException()
        {
            using (var sut = new DefaultDebugVisualizer())
            {
                sut.ShowPointerOperationEffect(_referenceObjects[0]);
                await UniTask.Delay(TimeSpan.FromSeconds(sut.IndicatorLifetime * 0.5f)); // dispose midway
            }

            LogAssert.Expect(LogType.Exception, "InvalidOperationException: Visualizer instance has been disposed.");

            await UniTask.Delay(TimeSpan.FromSeconds(_sut.IndicatorLifetime * 0.5f)); // wait for end of life
        }
    }
}
