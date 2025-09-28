// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiScrollWheelOperatorTest
    {
        private const string TestScene = "../../Scenes/ScrollViews.unity";

        private readonly IOperator _sut = new UguiScrollWheelOperator();
        private GameObject _scrollView;
        private GameObject _scrollViewHorizontal;
        private GameObject _scrollViewVertical;

        [SetUp]
        public void SetUp()
        {
            _scrollView = GameObject.Find("Both Scroll View");
            Centering(_scrollView);

            _scrollViewHorizontal = GameObject.Find("Horizontal Scroll View");
            Centering(_scrollViewHorizontal);

            _scrollViewVertical = GameObject.Find("Vertical Scroll View");
            Centering(_scrollViewVertical);
        }

        private static void Centering(GameObject scrollView)
        {
            if (scrollView == null)
            {
                return; // Some tests do not use LoadScene attribute, so _scrollView might be null.
            }

            var scrollRect = scrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f); // center the scroll view
        }

        [Test]
        public void Constructor_ValidScrollSpeed_ObjectCreatedSuccessfully()
        {
            var sut = new UguiScrollWheelOperator(100);
            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_ZeroScrollSpeed_ThrowsArgumentException()
        {
            Assert.That(() => new UguiScrollWheelOperator(0), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_NegativeScrollSpeed_ThrowsArgumentException()
        {
            Assert.That(() => new UguiScrollWheelOperator(-1), Throws.ArgumentException);
        }

        [Test]
        public void CanOperate_NullGameObject_ReturnsFalse()
        {
            var actual = _sut.CanOperate(null);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_DestroyedGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnScrollHandler));
            Object.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnScrollHandler));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(SpyOnScrollHandler));
            gameObject.transform.SetParent(parent.transform);
            parent.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_NoHandler_ReturnsFalse()
        {
            var gameObject = new GameObject();

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithEventTriggerNotIncludeScroll_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(EventTrigger));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithEventTriggerIncludeScroll_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateScrollEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledEventTriggerIncludeScroll_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateScrollEntry());
            eventTrigger.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        private static EventTrigger.Entry CreateScrollEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.Scroll,
                callback = new EventTrigger.TriggerEvent()
            };

        [Test]
        [CreateScene]
        public void CanOperate_WithScrollHandler_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(SpyOnScrollHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledScrollHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnScrollHandler));
            var handler = gameObject.GetComponent<SpyOnScrollHandler>();
            handler.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        public async Task OperateAsync_InvalidDirection_ThrowsArgumentException()
        {
            try
            {
                var sut = new UguiScrollWheelOperator();
                await sut.OperateAsync(null, Vector2.zero, 300);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message, Does.StartWith("Direction must not be zero"));
            }
        }

        [TestCase(-100)]
        [TestCase(0)]
        public async Task OperateAsync_InvalidDistance_ThrowsArgumentException(int distance)
        {
            try
            {
                var sut = new UguiScrollWheelOperator();
                await sut.OperateAsync(null, Vector2.up, distance);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message, Does.StartWith("Distance must be positive"));
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithScrollSpeed_ScrollSpecifiedAmountInOneFrame()
        {
            const int ScrollSpeed = 300;
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var rectTransform = content.GetComponent<RectTransform>();
            var beforePosition = rectTransform.position;

            var sut = new UguiScrollWheelOperator(ScrollSpeed);
            var task = sut.OperateAsync(_scrollView, Vector2.up, 300);
            await UniTask.NextFrame();

            var frameSpeed = ScrollSpeed * Time.deltaTime;
            var expectedPositionY = beforePosition.y - frameSpeed;
            Assert.That(rectTransform.position.y, Is.EqualTo(expectedPositionY).Within(10.0f));

            await task; // Ensure the task completes
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_Cancel_ScrollCancelled()
        {
            const int ScrollSpeed = 300;
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var rectTransform = content.GetComponent<RectTransform>();
            var beforePosition = rectTransform.position;

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var sut = new UguiScrollWheelOperator(ScrollSpeed);
            var task = sut.OperateAsync(_scrollView, Vector2.up, 300, cancellationToken: cancellationToken);
            await UniTask.NextFrame(cancellationToken);

            var frameSpeed = ScrollSpeed * Time.deltaTime;
            var expectedPositionY = beforePosition.y - frameSpeed;

            cancellationTokenSource.Cancel();
            await task; // Cancelled

            Assert.That(rectTransform.position.y, Is.EqualTo(expectedPositionY).Within(10.0f));
        }

        private static IEnumerable<TestCaseData> DirectionAndDistanceCases()
        {
            yield return new TestCaseData(Vector2.up, 100, new Vector2(0f, -100f));    // scroll down
            yield return new TestCaseData(Vector2.down, 100, new Vector2(0f, 100f));   // scroll up
            yield return new TestCaseData(Vector2.left, 100, new Vector2(100f, 0f));   // scroll right
            yield return new TestCaseData(Vector2.right, 100, new Vector2(-100f, 0f)); // scroll left
        }

        [TestCaseSource(nameof(DirectionAndDistanceCases))]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithDirectionAndDistance_Scrolled(Vector2 direction, int distance,
            Vector2 expectedDelta)
        {
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var rectTransform = content.GetComponent<RectTransform>();
            var beforePosition = rectTransform.position;
            var expectedPosition = beforePosition + new Vector3(expectedDelta.x, expectedDelta.y);

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView, direction, distance);

            Assert.That(rectTransform.position, Is.EqualTo(expectedPosition).Using(Vector3EqualityComparer.Instance));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_OnScrollCalled()
        {
            var spyEventHandler = _scrollView.AddComponent<SpyOnScrollHandler>();

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView, Vector2.up, 100);

            Assert.That(spyEventHandler.WasScrolled, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_OnPointerEnterAndExitCalled()
        {
            var spyEventHandler = _scrollView.AddComponent<SpyOnPointerEnterExitHandler>();

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView, Vector2.up, 100);

            Assert.That(spyEventHandler.WasPointerEntered, Is.True);
            Assert.That(spyEventHandler.WasPointerExited, Is.True);
        }

        #region Obsoleted

        [TestCase(0f, 0f)]
        [TestCase(30f, 20f)]
        [TestCase(-30f, -20f)]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithDestination_Scrolled(float x, float y)
        {
            var destination = new Vector2(x, y);
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var contentRectTransform = content.GetComponent<RectTransform>();
            var beforePosition = contentRectTransform.position;
            var expectedPosition = new Vector3(beforePosition.x + x, beforePosition.y + y, beforePosition.z);

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView, destination);

            Assert.That(contentRectTransform.position, Is.EqualTo(expectedPosition)
                .Using(new Vector3EqualityComparer(1.0f)));
        }

        #endregion

        [Test]
        [LoadScene(TestScene)]
        [Repeat(10)]
        public async Task OperateAsync_WithoutDirectionAndDistance_ScrollRectBoth_RandomScrolling()
        {
            var scrollRect = _scrollView.GetComponent<ScrollRect>();
            var beforePosition = scrollRect.normalizedPosition;

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView);

            var actual = scrollRect.normalizedPosition;
            Assert.That(actual, Is.Not.EqualTo(beforePosition));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(10)]
        public async Task OperateAsync_WithoutDirectionAndDistance_ScrollRectHorizontal_RandomScrollingHorizontal()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiScrollWheelOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewHorizontal);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(-1.00, 0.00)").Or.Contain("direction=(1.00, 0.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(10)]
        public async Task OperateAsync_WithoutDirectionAndDistance_ScrollRectVertical_RandomScrollingVertical()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiScrollWheelOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewVertical);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(0.00, -1.00)").Or.Contain("direction=(0.00, 1.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(10)]
        public async Task OperateAsync_WithoutDirectionAndDistance_ScrollbarHorizontal_RandomScrollingHorizontal()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiScrollWheelOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewHorizontal.GetComponent<ScrollRect>().horizontalScrollbar.gameObject);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(-1.00, 0.00)").Or.Contain("direction=(1.00, 0.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(10)]
        public async Task OperateAsync_WithoutDirectionAndDistance_ScrollbarVertical_RandomScrollingVertical()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiScrollWheelOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewVertical.GetComponent<ScrollRect>().verticalScrollbar.gameObject);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(0.00, -1.00)").Or.Contain("direction=(0.00, 1.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(10)]
        public async Task OperateAsync_WithoutDirectionAndDistance_NotScrollRectOrScrollbar_RandomScrolling()
        {
            var gameObject = new GameObject(null, typeof(Image));
            var spyOnScrollHandler = gameObject.AddComponent<SpyOnScrollHandler>();

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(gameObject);

            Assert.That(spyOnScrollHandler.LastScrollDelta.magnitude, Is.GreaterThan(0f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithLogger_LoggingWorks()
        {
            var spyLogger = new SpyLogger();

            var sut = new UguiScrollWheelOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollView);

            Assert.That(spyLogger.Messages, Is.Not.Empty);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithScreenshotOptions_TakeScreenshot()
        {
            var directory = Application.temporaryCachePath;
            var filename = $"{TestContext.CurrentContext.Test.FullName}.png";
            var path = Path.Combine(directory, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var screenshotOptions = new ScreenshotOptions
            {
                Directory = directory,
                FilenameStrategy = new StubScreenshotFilenameStrategy(filename),
            };

            var sut = new UguiScrollWheelOperator(screenshotOptions: screenshotOptions);
            await sut.OperateAsync(_scrollView);

            Assert.That(path, Does.Exist);
        }
    }
}
