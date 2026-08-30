// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Paginators
{
    [TestFixture]
    public class UguiScrollRectPaginatorTest
    {
        private const string TestScene = "../../Scenes/ScrollViews.unity";

        private GameObject _horizontalScrollView;
        private GameObject _verticalScrollView;
        private GameObject _bothScrollView;

        [SetUp]
        public void SetUp()
        {
            _horizontalScrollView = GameObject.Find("Horizontal Scroll View");
            _verticalScrollView = GameObject.Find("Vertical Scroll View");
            _bothScrollView = GameObject.Find("Both Scroll View");
        }

        [Test]
        [LoadScene(TestScene)]
        public void Constructor_ValidScrollRect_ObjectCreatedSuccessfully()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            var sut = new UguiScrollRectPaginator(scrollRect);

            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_NullScrollRect_ThrowsArgumentNullException()
        {
            Assert.That(() => new UguiScrollRectPaginator(null), Throws.ArgumentNullException);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_HorizontalScrollRect_NormalizedPositionBecomesZero()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            await sut.ResetAsync();

            Assert.That(scrollRect.normalizedPosition.x, Is.EqualTo(0f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_VerticalScrollRect_NormalizedPositionBecomesOne()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            await sut.ResetAsync();

            Assert.That(scrollRect.normalizedPosition.y, Is.EqualTo(1f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task ResetAsync_BothScrollRect_NormalizedPositionBecomesZeroOne()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            await sut.ResetAsync();

            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(0f, 1f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_HorizontalScrollNotAtEnd_ScrollsHorizontallyAndReturnsTrue()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.x, Is.GreaterThan(beforePosition.x));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_HorizontalScrollAtEnd_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition.x, Is.EqualTo(1f).Within(float.Epsilon));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_VerticalScrollNotAtEnd_ScrollsVerticallyAndReturnsTrue()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.y, Is.LessThan(beforePosition.y));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_VerticalScrollAtEnd_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition.y, Is.EqualTo(0f).Within(float.Epsilon));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_BothScrollHorizontalNotAtEnd_ScrollsHorizontallyAndReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0.5f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.x, Is.GreaterThan(beforePosition.x));
            Assert.That(scrollRect.normalizedPosition.y, Is.EqualTo(beforePosition.y));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_BothScrollHorizontalAtEnd_ResetsXAndScrollsVerticallyAndReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True);
            Assert.That(scrollRect.normalizedPosition.x, Is.EqualTo(0f).Within(float.Epsilon));
            Assert.That(scrollRect.normalizedPosition.y, Is.LessThan(1f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_BothScrollBothAtEnd_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(1f, 0f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_ScrollDisabled_DoesNotScrollAndReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False);
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(new Vector2(0f, 1f)));
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        [CreateScene]
        [Category("Acceptance")]
        public async Task NextPageAsync_ViewportSizeIsZero_ReturnsFalse(bool horizontal, bool vertical)
        {
            // A zero-size viewport reproduces the state before Unity's layout calculation.
            var scrollRect = CreateScrollRect(Vector2.zero, new Vector2(500f, 500f));
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False, "return value");
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(beforePosition), "normalizedPosition");
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        [CreateScene]
        public async Task NextPageAsync_ContentFitsViewport_ReturnsFalse(bool horizontal, bool vertical)
        {
            var scrollRect = CreateScrollRect(new Vector2(100f, 100f), new Vector2(50f, 50f));
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False, "return value");
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(beforePosition), "normalizedPosition");
        }

        [Test]
        [CreateScene]
        public async Task NextPageAsync_BothScrollHorizontalContentFitsViewport_ScrollsVerticallyAndReturnsTrue()
        {
            var scrollRect = CreateScrollRect(new Vector2(100f, 100f), new Vector2(50f, 500f));
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True, "return value");
            Assert.That(scrollRect.normalizedPosition.x, Is.EqualTo(beforePosition.x), "normalizedPosition.x");
            Assert.That(scrollRect.normalizedPosition.y, Is.LessThan(beforePosition.y), "normalizedPosition.y");
        }

        [Test]
        [CreateScene]
        public async Task NextPageAsync_BothScrollHorizontalAtEndAndVerticalContentFitsViewport_ReturnsFalse()
        {
            var scrollRect = CreateScrollRect(new Vector2(100f, 100f), new Vector2(500f, 50f));
            // Shift content down so that verticalNormalizedPosition reads 1 (not at end) although the axis cannot advance.
            scrollRect.content.anchoredPosition = new Vector2(0f, -100f);
            scrollRect.horizontalNormalizedPosition = 1f;
            Assume.That(scrollRect.verticalNormalizedPosition, Is.EqualTo(1f));
            var sut = new UguiScrollRectPaginator(scrollRect);
            var beforePosition = scrollRect.normalizedPosition;

            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.False, "return value");
            Assert.That(scrollRect.normalizedPosition, Is.EqualTo(beforePosition), "normalizedPosition");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task NextPageAsync_AfterResetAsyncFollowingHorizontalEnd_ScrollsHorizontallyAndReturnsTrue()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.99f, 1f);
            var sut = new UguiScrollRectPaginator(scrollRect);
            Assume.That(await sut.NextPageAsync(), Is.True);
            Assume.That(scrollRect.normalizedPosition.x, Is.EqualTo(1f).Within(float.Epsilon));

            await sut.ResetAsync();
            var actual = await sut.NextPageAsync();

            Assert.That(actual, Is.True, "return value");
            Assert.That(scrollRect.normalizedPosition.x, Is.GreaterThan(0f), "normalizedPosition.x");
            Assert.That(scrollRect.normalizedPosition.y, Is.EqualTo(1f), "normalizedPosition.y");
        }

        private static ScrollRect CreateScrollRect(Vector2 viewportSize, Vector2 contentSize)
        {
            var scrollRect = new GameObject("Scroll View", typeof(RectTransform)).AddComponent<ScrollRect>();
            ((RectTransform)scrollRect.transform).sizeDelta = viewportSize;
            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(scrollRect.transform, false);
            content.sizeDelta = contentSize;
            scrollRect.content = content;
            return scrollRect;
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [LoadScene(TestScene)]
        public void HasNextPage_HorizontalScrollNotAtEnd_ReturnsTrue(float x)
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(x, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_HorizontalScrollAtEnd_ReturnsFalse()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [TestCase(1f)]
        [TestCase(0.5f)]
        [LoadScene(TestScene)]
        public void HasNextPage_VerticalScrollNotAtEnd_ReturnsTrue(float y)
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, y);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_VerticalScrollAtEnd_ReturnsFalse()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [TestCase(0f, 1f)]
        [TestCase(1f, 0.5f)]
        [TestCase(0.5f, 0f)]
        [LoadScene(TestScene)]
        public void HasNextPage_BothScrollNotBothAtEnd_ReturnsTrue(float x, float y)
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(x, y);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_BothScrollBothAtEnd_ReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1f, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_ScrollDisabled_ReturnsFalse()
        {
            var scrollRect = _bothScrollView.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = false;
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_HorizontalFloatingPointPrecisionAtEnd_ReturnsFalse()
        {
            var scrollRect = _horizontalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(1.0f - float.Epsilon, 0f);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void HasNextPage_VerticalFloatingPointPrecisionAtEnd_ReturnsFalse()
        {
            var scrollRect = _verticalScrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0f, 0.0f + float.Epsilon);
            var sut = new UguiScrollRectPaginator(scrollRect);

            var actual = sut.HasNextPage();

            Assert.That(actual, Is.False);
        }
    }
}
