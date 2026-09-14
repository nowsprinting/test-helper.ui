// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestUtils;
using UnityEngine;

namespace TestHelper.UI.Strategies.Utilities
{
    [TestFixture]
    [Category("Internal")]
    public class ScreenRectUtilityTest
    {
        private const string TestScenePath = "../../../Scenes/GameObjectFinderUI.unity";

        private static readonly Rect s_rect = new Rect(0, 0, 100, 100);

        private static readonly TestCaseData[] s_overlappingCases =
        {
            new TestCaseData(s_rect, new Rect(50, 50, 100, 100), new Rect(50, 50, 50, 50))
                .SetName("{m}(PartialOverlap)"),
            new TestCaseData(s_rect, new Rect(25, 25, 10, 10), new Rect(25, 25, 10, 10))
                .SetName("{m}(OneContainsTheOther)"),
        };

        [TestCaseSource(nameof(s_overlappingCases))]
        public void Intersect_Overlapping_ReturnsOverlapArea(Rect a, Rect b, Rect expected)
        {
            var actual = ScreenRectUtility.Intersect(a, b);

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static readonly TestCaseData[] s_disjointCases =
        {
            new TestCaseData(s_rect, new Rect(200, 0, 10, 10)).SetName("{m}(Separated)"),
            new TestCaseData(s_rect, new Rect(100, 0, 10, 10)).SetName("{m}(TouchingEdges)"),
        };

        [TestCaseSource(nameof(s_disjointCases))]
        public void Intersect_Disjoint_ReturnsRectUnderOnePixel(Rect a, Rect b)
        {
            var actual = ScreenRectUtility.Intersect(a, b);

            Assert.That(Mathf.Min(actual.width, actual.height), Is.LessThan(1f));
        }

        private static readonly TestCaseData[] s_disjointBlockerCases =
        {
            new TestCaseData(new Rect(200, 0, 10, 10)).SetName("{m}(Separated)"),
            new TestCaseData(new Rect(100, 0, 10, 10)).SetName("{m}(TouchingEdge)"),
        };

        [TestCaseSource(nameof(s_disjointBlockerCases))]
        public void LargestRemainder_DisjointBlocker_ReturnsRectUnchanged(Rect blocker)
        {
            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(s_rect));
        }

        private static readonly TestCaseData[] s_coveringBlockerCases =
        {
            new TestCaseData(new Rect(0, 0, 100, 100)).SetName("{m}(EqualToRect)"),
            new TestCaseData(new Rect(-10, -10, 120, 120)).SetName("{m}(LargerThanRect)"),
            new TestCaseData(new Rect(0.5f, 0, 100, 100)).SetName("{m}(LeavesStripUnderOnePixel)"),
        };

        [TestCaseSource(nameof(s_coveringBlockerCases))]
        public void LargestRemainder_BlockerCoversRect_ReturnsDegenerateRect(Rect blocker)
        {
            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(Mathf.Min(actual.width, actual.height), Is.LessThan(1f));
        }

        [Test]
        public void LargestRemainder_LeftStripLargest_ReturnsLeftStrip()
        {
            var blocker = new Rect(60, 10, 50, 50); // left 60x100, bottom 100x10, top 100x40

            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(new Rect(0, 0, 60, 100)));
        }

        [Test]
        public void LargestRemainder_RightStripLargest_ReturnsRightStrip()
        {
            var blocker = new Rect(-10, 10, 50, 50); // right 60x100, bottom 100x10, top 100x40

            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(new Rect(40, 0, 60, 100)));
        }

        [Test]
        public void LargestRemainder_BottomStripLargest_ReturnsBottomStrip()
        {
            var blocker = new Rect(10, 60, 50, 50); // left 10x100, right 40x100, bottom 100x60

            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(new Rect(0, 0, 100, 60)));
        }

        [Test]
        public void LargestRemainder_TopStripLargest_ReturnsTopStrip()
        {
            var blocker = new Rect(10, -10, 50, 50); // left 10x100, right 40x100, top 100x60

            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(new Rect(0, 40, 100, 60)));
        }

        private static readonly TestCaseData[] s_leftStripTieCases =
        {
            new TestCaseData(new Rect(40, 0, 20, 100)).SetName("{m}(LeftAndRight)"), // left 40x100, right 40x100
            new TestCaseData(new Rect(40, 40, 20, 20)).SetName("{m}(AllStrips)"),    // every strip has area 4000
        };

        [TestCaseSource(nameof(s_leftStripTieCases))]
        public void LargestRemainder_LeftStripTiesWithOthers_ReturnsLeftStrip(Rect blocker)
        {
            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(new Rect(0, 0, 40, 100)));
        }

        [Test]
        public void LargestRemainder_BottomAndTopStripsTie_ReturnsBottomStrip()
        {
            var blocker = new Rect(0, 40, 100, 20); // bottom 100x40, top 100x40

            var actual = ScreenRectUtility.LargestRemainder(s_rect, blocker);

            Assert.That(actual, Is.EqualTo(new Rect(0, 0, 100, 40)));
        }

        [Test]
        [CreateScene]
        public void TryGetScreenRect_NoRectTransform_ReturnsFalse()
        {
            var gameObject = new GameObject("NoRectTransform");

            var actual = ScreenRectUtility.TryGetScreenRect(gameObject, out _);

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScenePath)]
        public void TryGetScreenRect_ObjectOnScreenSpaceOverlayCanvas_ReturnsProjectedRect()
        {
            var target = GameObject.Find("ActiveText"); // 160x30 at (0,100) on VGA

            var actual = ScreenRectUtility.TryGetScreenRect(target, out var rect);

            Assert.That(actual, Is.True, "returns true");
            Assert.That(rect, Is.EqualTo(new Rect(240, 325, 160, 30)), "rect");
        }

        [Test]
        [LoadScene(TestScenePath)]
        public void TryGetScreenRect_ObjectOnWorldSpaceCanvas_ReturnsRectProjectedByCanvasCamera()
        {
            var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.transform.position = new Vector3(0, 0, 500);
            var target = GameObject.Find("ActiveText");
            var expected = ScreenRectTestHelper.GetScreenRect(target);

            var actual = ScreenRectUtility.TryGetScreenRect(target, out var rect);

            Assert.That(actual, Is.True, "returns true");
            Assert.That(rect, Is.EqualTo(expected), "rect");
        }
    }
}
