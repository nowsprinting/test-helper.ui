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
    public class UguiSwipeOperatorTest
    {
        private const string TestScene = "../../Scenes/ScrollViews.unity";
        private const int RandomTestRepeatCount = 10;

        private readonly ISwipeOperator _sut = new UguiSwipeOperator();
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
        public void Constructor_ValidSwipeSpeedAndDistance_ObjectCreatedSuccessfully()
        {
            var sut = new UguiSwipeOperator(swipeSpeed: 1, swipeDistance: 2);
            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_InvalidSwipeSpeed_ThrowsArgumentException([Values(-1, 0)] int swipeSpeed)
        {
            Assert.That(() => new UguiSwipeOperator(swipeSpeed: swipeSpeed),
                Throws.TypeOf<ArgumentException>().With.Message.Contains("Swipe speed must be positive"));
        }

        [Test]
        public void Constructor_InvalidSwipeDistance_ThrowsArgumentException([Values(-1f, 0f)] float swipeDistance)
        {
            Assert.That(() => new UguiSwipeOperator(swipeDistance: swipeDistance),
                Throws.TypeOf<ArgumentException>().With.Message.Contains("Swipe distance must be positive"));
        }

        [Test]
        [CreateScene]
        public void CanOperate_DragHandler_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(SpyOnDragHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_PointerDownAndUpHandlers_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_PointerDownHandlerOnly_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_PointerUpHandlerOnly_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerUpHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_NothingHandler_ReturnsFalse()
        {
            var gameObject = new GameObject();

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_DisabledHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnDragHandler));
            var handler = gameObject.GetComponent<SpyOnDragHandler>();
            handler.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
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
            var gameObject = new GameObject(null, typeof(SpyOnDragHandler));
            Object.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnDragHandler));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParent_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(SpyOnDragHandler));
            gameObject.transform.SetParent(parent.transform);
            parent.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_EmptyEventTrigger_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(EventTrigger));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_DisabledEventTrigger_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateDragEntry());
            eventTrigger.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithEventTriggerIncludeDrag_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateDragEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        private static EventTrigger.Entry CreateDragEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag,
                callback = new EventTrigger.TriggerEvent()
            };

        [Test]
        [CreateScene]
        public void CanOperate_EventTriggerIncludePointerDownOnly_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerDownEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_EventTriggerIncludePointerUpOnly_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerUpEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_EventTriggerIncludePointerDownAndUp_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerDownEntry());
            eventTrigger.triggers.Add(CreatePointerUpEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        private static EventTrigger.Entry CreatePointerDownEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerDown,
                callback = new EventTrigger.TriggerEvent()
            };

        private static EventTrigger.Entry CreatePointerUpEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerUp,
                callback = new EventTrigger.TriggerEvent()
            };

        [Test]
        public async Task OperateAsync_InvalidDirection_ThrowsArgumentException()
        {
            try
            {
                await _sut.OperateAsync(null, Vector2.zero);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message, Does.StartWith("Direction must not be zero"));
            }
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_SpecifySwipeSpeed_SwipeSpecifiedAmountInOneFrame()
        {
            const int SwipeSpeed = 200;
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var rectTransform = content.GetComponent<RectTransform>();
            var beforePosition = rectTransform.position;

            var sut = new UguiSwipeOperator(swipeSpeed: SwipeSpeed);
            var task = sut.OperateAsync(_scrollView, Vector2.up);
            await UniTask.NextFrame();

            var frameSpeed = SwipeSpeed * Time.deltaTime;
            var expectedPositionY = beforePosition.y - frameSpeed;
            Assert.That(rectTransform.position.y, Is.EqualTo(expectedPositionY).Within(30.0f));

            await task; // Ensure the task completes
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_Cancel_SwipeCancelled()
        {
            const int SwipeSpeed = 200;
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var rectTransform = content.GetComponent<RectTransform>();
            var beforePosition = rectTransform.position;

            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var sut = new UguiSwipeOperator(swipeSpeed: SwipeSpeed);
            var task = sut.OperateAsync(_scrollView, Vector2.up, cancellationToken: cancellationToken);
            await UniTask.NextFrame(cancellationToken);

            var frameSpeed = SwipeSpeed * Time.deltaTime;
            var expectedPositionY = beforePosition.y - frameSpeed;

            cancellationTokenSource.Cancel();
            await task; // Cancelled

            Assert.That(rectTransform.position.y, Is.EqualTo(expectedPositionY).Within(10.0f));
        }

        private static IEnumerable<TestCaseData> DirectionAndDistanceCases()
        {
            yield return new TestCaseData(Vector2.up, 100, new Vector2(0f, 100f));    // move up
            yield return new TestCaseData(Vector2.down, 100, new Vector2(0f, -100f)); // move down
            yield return new TestCaseData(Vector2.left, 100, new Vector2(-100f, 0f)); // move left
            yield return new TestCaseData(Vector2.right, 100, new Vector2(100f, 0f)); // move right
        }

        [TestCaseSource(nameof(DirectionAndDistanceCases))]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithDirection_Swiped(Vector2 direction, int distance, Vector2 expectedDelta)
        {
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var rectTransform = content.GetComponent<RectTransform>();
            var beforePosition = rectTransform.position;
            var expectedPosition = beforePosition + new Vector3(expectedDelta.x, expectedDelta.y);

            var sut = new UguiSwipeOperator(swipeDistance: distance);
            await sut.OperateAsync(_scrollView, direction);

            Assert.That(rectTransform.position, Is.EqualTo(expectedPosition).Using(new Vector3EqualityComparer(10.0f)));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_OnDragCalled()
        {
            var spyDragHandler = _scrollView.AddComponent<SpyOnDragHandler>();

            var sut = new UguiSwipeOperator();
            await sut.OperateAsync(_scrollView, Vector2.up);

            Assert.That(spyDragHandler.WasInitializePotentialDrag, Is.True);
            Assert.That(spyDragHandler.WasBeginDrag, Is.True);
            Assert.That(spyDragHandler.WasEndDrag, Is.True);
            Assert.That(spyDragHandler.LastDragPosition, Is.Not.EqualTo(SpyOnDragHandler.LastDragPositionInitialValue));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_OnPointerDownAndUpCalled()
        {
            var spyOnPointerDownHandler = _scrollView.AddComponent<SpyOnPointerDownHandler>();
            var spyOnPointerUpHandler = _scrollView.AddComponent<SpyOnPointerUpHandler>();

            var sut = new UguiSwipeOperator();
            await sut.OperateAsync(_scrollView, Vector2.up);

            Assert.That(spyOnPointerDownHandler.WasOnPointerDown, Is.True);
            Assert.That(spyOnPointerUpHandler.WasOnPointerUp, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_WithoutDirection_ScrollRectBoth_RandomSwipe()
        {
            const int SwipeDistance = 200;
            var scrollRect = _scrollView.GetComponent<ScrollRect>();
            var beforePosition = scrollRect.normalizedPosition;

            var sut = new UguiSwipeOperator(swipeDistance: SwipeDistance);
            await sut.OperateAsync(_scrollView);

            var actual = scrollRect.normalizedPosition;
            Assert.That(actual, Is.Not.EqualTo(beforePosition));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_WithoutDirection_ScrollRectHorizontal_RandomSwipeHorizontal()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiSwipeOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewHorizontal);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(-1.00, 0.00)").Or.Contain("direction=(1.00, 0.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_WithoutDirection_ScrollRectVertical_RandomSwipeVertical()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiSwipeOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewVertical);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(0.00, -1.00)").Or.Contain("direction=(0.00, 1.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_WithoutDirection_ScrollbarHorizontal_RandomSwipeHorizontal()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiSwipeOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewHorizontal.GetComponent<ScrollRect>().horizontalScrollbar.gameObject);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(-1.00, 0.00)").Or.Contain("direction=(1.00, 0.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_WithoutDirection_ScrollbarVertical_RandomSwipeVertical()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiSwipeOperator(logger: spyLogger);
            await sut.OperateAsync(_scrollViewVertical.GetComponent<ScrollRect>().verticalScrollbar.gameObject);

            Assert.That(spyLogger.Messages[0], Does.Contain(
                "direction=(0.00, -1.00)").Or.Contain("direction=(0.00, 1.00)"));
        }

        [Test]
        [LoadScene(TestScene)]
        [Repeat(RandomTestRepeatCount)]
        public async Task OperateAsync_WithoutDirection_NotScrollRectOrScrollbar_RandomSwipe()
        {
            const int SwipeDistance = 200;
            var gameObject = new GameObject();
            var image = gameObject.AddComponent<Image>();
            var beforePosition = image.transform.position;
            var spyOnDragHandler = gameObject.AddComponent<SpyOnDragHandler>();

            var sut = new UguiSwipeOperator(swipeDistance: SwipeDistance);
            await sut.OperateAsync(gameObject);

            var actual = Vector3.Distance(spyOnDragHandler.LastDragPosition, beforePosition);
            Assert.That(actual, Is.GreaterThan(0f));
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithLogger_LoggingWorks()
        {
            var spyLogger = new SpyLogger();

            var sut = new UguiSwipeOperator(logger: spyLogger);
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

            var sut = new UguiSwipeOperator(screenshotOptions: screenshotOptions);
            await sut.OperateAsync(_scrollView);

            Assert.That(path, Does.Exist);
        }
    }
}
