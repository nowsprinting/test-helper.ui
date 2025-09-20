// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

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

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiScrollWheelOperatorTest
    {
        private const string TestScene = "../../Scenes/ScrollViews.unity";

        private readonly IOperator _sut = new UguiScrollWheelOperator();
        private GameObject _scrollView;

        [SetUp]
        public void SetUp()
        {
            _scrollView = GameObject.Find("Both Scroll View");
            if (_scrollView == null)
            {
                return; // Some tests do not use LoadScene attribute, so _scrollView might be null.
            }

            var scrollRect = _scrollView.GetComponent<ScrollRect>();
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f); // center the scroll view
        }

        [Test]
        public void Constructor_ValidScrollSpeed_ObjectCreatedSuccessfully()
        {
            var sut = new UguiScrollWheelOperator(1.0f);
            Assert.That(sut, Is.Not.Null);
        }

        [Test]
        public void Constructor_ZeroScrollSpeed_ThrowsArgumentException()
        {
            Assert.That(() => new UguiScrollWheelOperator(0f), Throws.ArgumentException);
        }

        [Test]
        public void Constructor_NegativeScrollSpeed_ThrowsArgumentException()
        {
            Assert.That(() => new UguiScrollWheelOperator(-1.0f), Throws.ArgumentException);
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
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithScrollSpeed_ScrollSpecifiedAmountInOneFrame()
        {
            const float ScrollSpeed = 5.0f;
            var destination = new Vector2(20, 20);
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var contentRectTransform = content.GetComponent<RectTransform>();
            var beforePosition = contentRectTransform.position;
            var expectedPosition = new Vector3(beforePosition.x + ScrollSpeed, beforePosition.y + ScrollSpeed,
                beforePosition.z);

            var sut = new UguiScrollWheelOperator(ScrollSpeed);
            var task = sut.OperateAsync(_scrollView, destination);
            await UniTask.NextFrame();

            Assert.That(contentRectTransform.position, Is.EqualTo(expectedPosition)
                .Using(new Vector3EqualityComparer(1.0f)));

            await task; // Ensure the task completes
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_Cancel_ScrollCancelled()
        {
            const float ScrollSpeed = 5.0f;
            var destination = new Vector2(20, 20);
            var viewport = _scrollView.transform.Find("Viewport");
            var content = viewport.Find("Content");
            var contentRectTransform = content.GetComponent<RectTransform>();
            var beforePosition = contentRectTransform.position;
            var expectedPosition = new Vector3(beforePosition.x + ScrollSpeed, beforePosition.y + ScrollSpeed,
                beforePosition.z); // Cancelled position

            var sut = new UguiScrollWheelOperator(ScrollSpeed);
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            var task = sut.OperateAsync(_scrollView, destination, cancellationToken: cancellationToken);
            await UniTask.NextFrame(cancellationToken);

            cancellationTokenSource.Cancel();
            await task; // Cancelled

            Assert.That(contentRectTransform.position, Is.EqualTo(expectedPosition)
                .Using(new Vector3EqualityComparer(1.0f)));
        }

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

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_OnScrollCalled()
        {
            var spyEventHandler = _scrollView.AddComponent<SpyOnScrollHandler>();

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView, new Vector2(2f, 3f));

            Assert.That(spyEventHandler.WasScrolled, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_OnPointerEnterAndExitCalled()
        {
            var spyEventHandler = _scrollView.AddComponent<SpyOnPointerEnterExitHandler>();

            var sut = new UguiScrollWheelOperator();
            await sut.OperateAsync(_scrollView, new Vector2(2f, 3f));

            Assert.That(spyEventHandler.WasPointerEntered, Is.True);
            Assert.That(spyEventHandler.WasPointerExited, Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task OperateAsync_WithoutDestination_RandomScrolling()
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
