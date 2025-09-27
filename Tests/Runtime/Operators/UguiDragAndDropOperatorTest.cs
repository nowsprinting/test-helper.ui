// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.Annotations;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiDragAndDropOperatorTest
    {
        private readonly IOperator _sut = new UguiDragAndDropOperator();
        private Vector3 _objectPosition;

        [SetUp]
        public void SetUp()
        {
            _objectPosition = new Vector3(50, 50, 0);
        }

        private void SetOnCanvas(MonoBehaviour handler)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            var canvas = GameObject.FindObjectOfType<Canvas>();
#pragma warning restore CS0618 // Type or member is obsolete
            Assume.That(canvas, Is.Not.Null, "Canvas not found in the scene.");

            handler.transform.SetParent(canvas.transform);
            handler.transform.position = _objectPosition;
            _objectPosition.x += ((RectTransform)handler.transform).sizeDelta.x;
            _objectPosition.y += ((RectTransform)handler.transform).sizeDelta.y;
        }

        private SpyOnDragHandler CreateSpyDragHandler()
        {
            var handler = new GameObject("SpyDragHandler").AddComponent<SpyOnDragHandler>();
            SetOnCanvas(handler);
            return handler;
        }

        private SpyOnDropHandler CreateSpyDropHandler()
        {
            var handler = new GameObject("SpyDropHandler").AddComponent<SpyOnDropHandler>();
            SetOnCanvas(handler);
            return handler;
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
            var gameObject = new GameObject(null, typeof(DummyDragHandler));
            GameObject.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(DummyDragHandler));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(DummyDragHandler));
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
        public void CanOperate_WithEventTriggerNotIncludeDrag_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(EventTrigger));

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

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledEventTriggerIncludeDrag_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateDragEntry());
            eventTrigger.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        private static EventTrigger.Entry CreateDragEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag,
                callback = new EventTrigger.TriggerEvent()
            };

        [Test]
        [CreateScene]
        public void CanOperate_WithPointerDragHandler_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(DummyDragHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledPointerDragHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(DummyDragHandler));
            var handler = gameObject.GetComponent<DummyDragHandler>();
            handler.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void LotteryComponent_Empty_ReturnsNull()
        {
            var components = Array.Empty<Component>();
            var sut = new UguiDragAndDropOperator();
            var actual = sut.LotteryComponent(components);

            Assert.That(actual, Is.Null);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task LotteryComponent_OnlyNotReachable_ReturnsNull()
        {
            var notReachableHandler = CreateSpyDropHandler();
            var notReachableAnnotation = notReachableHandler.gameObject.AddComponent<DropAnnotation>();
            var blocker = CreateSpyDropHandler();
            blocker.transform.position = notReachableHandler.transform.position;
            var components = new Component[] { notReachableAnnotation };

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            var actual = sut.LotteryComponent(components);

            Assert.That(actual, Is.Null);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        [Repeat(5)]
        public async Task LotteryComponent_IncludeValidComponent_ReturnsComponent()
        {
            var notReachableHandler = CreateSpyDropHandler();
            var notReachableAnnotation = notReachableHandler.gameObject.AddComponent<DropAnnotation>();
            var blocker = CreateSpyDropHandler();
            blocker.transform.position = notReachableHandler.transform.position;

            var reachableHandler = CreateSpyDropHandler();
            var reachableAnnotation = reachableHandler.gameObject.AddComponent<DropAnnotation>();
            var components = new Component[] { notReachableAnnotation, reachableAnnotation };

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            var actual = sut.LotteryComponent(components);

            Assert.That(actual, Is.EqualTo(reachableAnnotation));
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        [Repeat(5)]
        public async Task OperateAsync_ExistDropAnnotation_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler(); // dummy
            var dropHandlerWithAnnotation = CreateSpyDropHandler();
            dropHandlerWithAnnotation.gameObject.AddComponent<DropAnnotation>();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject);

            Assert.That(dropHandler.WasDrop, Is.False);
            Assert.That(dropHandlerWithAnnotation.WasDrop, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_ExistDropHandlerObject_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject);

            Assert.That(dropHandler.WasDrop, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_NotExistDropTarget_DropOnRandomScreenPoint()
        {
            var dragHandler = CreateSpyDragHandler();
            var pointerDownHandler = dragHandler.gameObject.AddComponent<SpyOnPointerDownHandler>();
            var pointerUpHandler = dragHandler.gameObject.AddComponent<SpyOnPointerUpHandler>();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject);

            Assert.That(pointerDownHandler.WasOnPointerDown, Is.True);
            Assert.That(dragHandler.WasInitializePotentialDrag, Is.True);
            Assert.That(dragHandler.WasBeginDrag, Is.True);
            Assert.That(dragHandler.WasEndDrag, Is.True);
            Assert.That(dragHandler.LastDragPosition, Is.Not.EqualTo(default(Vector2)));
            Assert.That(pointerUpHandler.WasOnPointerUp, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_SpecifyDropTargetObject_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject, dropHandler.gameObject);

            Assert.That(dropHandler.WasDrop, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_SpecifyDropTargetPoint_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject, dropHandler.transform.position);

            Assert.That(dropHandler.WasDrop, Is.True);
        }
    }
}
