// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Paginators;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Performance
{
    [TestFixture]
    public class GameObjectFinderTest
    {
        private const int ButtonCount = 1000;
        private const int NestDepth = 10;
        private static readonly string s_targetButtonName = $"Button_{ButtonCount - 1}";

        [Test]
        [Performance]
        [CreateScene(camera: true)]
        public async Task FindByMatcherAsync_ButtonMatcherAmongManyGameObjects_MeasureTimeAndAllocations()
        {
            UiFixtureFactory.CreateNestedButtons(ButtonCount, NestDepth);

            var finder = new GameObjectFinder();
            var matcher = new ButtonMatcher(text: s_targetButtonName);

            // reachable: false because the generated buttons have no Image, so raycasts can never hit them;
            // the subject here is the hierarchy scan and matcher, not reachability.
            await PerformanceMeasurement.MeasureAsync(() => finder.FindByMatcherAsync(matcher, reachable: false));
        }

        [Test]
        [Performance]
        [CreateScene(camera: true)]
        public async Task FindByNameAsync_AmongManyGameObjects_MeasureTimeAndAllocations()
        {
            UiFixtureFactory.CreateNestedButtons(ButtonCount, NestDepth);

            var finder = new GameObjectFinder();

            await PerformanceMeasurement.MeasureAsync(() =>
                finder.FindByNameAsync(s_targetButtonName, reachable: false));
        }

        [Test]
        [Performance]
        [LoadScene("../Scenes/ScrollViews.unity")]
        public async Task FindByMatcherAsync_WithPaginatorInScrollView_MeasureTimeAndAllocations()
        {
            var scrollView = GameObject.Find("Vertical Scroll View");
            Assume.That(scrollView, Is.Not.Null);

            var finder = new GameObjectFinder(timeoutSeconds: 5.0d);
            // The last button in the scroll view, so every iteration sweeps all pages.
            var matcher = new ButtonMatcher(name: "Vertical_Button_29");
            var paginator = new UguiScrollRectPaginator(scrollView.GetComponent<ScrollRect>());

            await PerformanceMeasurement.MeasureAsync(() =>
                finder.FindByMatcherAsync(matcher, reachable: true, paginator: paginator));
        }
    }
}
