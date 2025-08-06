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

        private SpyDragHandler CreateSpyDragHandler()
        {
            var handler = new GameObject("SpyDragHandler").AddComponent<SpyDragHandler>();
            SetOnCanvas(handler);
            return handler;
        }

        private SpyDropHandler CreateSpyDropHandler()
        {
            var handler = new GameObject("SpyDropHandler").AddComponent<SpyDropHandler>();
            SetOnCanvas(handler);
            return handler;
        }

        [Test]
        public void CanOperate_Null_ReturnsFalse()
        {
            var sut = new UguiDragAndDropOperator();
            var actual = sut.CanOperate(null);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void CanOperate_HasNotDragHandler_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var sut = new UguiDragAndDropOperator();
            var actual = sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public void CanOperate_HasDragHandler_ReturnsTrue()
        {
            var gameObject = CreateSpyDragHandler().gameObject;
            var sut = new UguiDragAndDropOperator();
            var actual = sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        public void LotteryDropAnnotation_Empty_ReturnsNull()
        {
            var annotations = Array.Empty<DropAnnotation>();
            var sut = new UguiDragAndDropOperator();
            var actual = sut.LotteryDropAnnotation(annotations);

            Assert.That(actual, Is.Null);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task LotteryDropAnnotation_OnlyNotReachable_ReturnsNull()
        {
            var notReachableHandler = CreateSpyDropHandler();
            var notReachableAnnotation = notReachableHandler.gameObject.AddComponent<DropAnnotation>();
            var blocker = CreateSpyDropHandler();
            blocker.transform.position = notReachableHandler.transform.position;
            var annotations = new[] { notReachableAnnotation };

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            var actual = sut.LotteryDropAnnotation(annotations);

            Assert.That(actual, Is.Null);
        }

        [Test]
        [Repeat(10)]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task LotteryDropAnnotation_IncludeValidAnnotation_ReturnsDropAnnotation()
        {
            var notReachableHandler = CreateSpyDropHandler();
            var notReachableAnnotation = notReachableHandler.gameObject.AddComponent<DropAnnotation>();
            var blocker = CreateSpyDropHandler();
            blocker.transform.position = notReachableHandler.transform.position;

            var reachableHandler = CreateSpyDropHandler();
            var reachableAnnotation = reachableHandler.gameObject.AddComponent<DropAnnotation>();
            var annotations = new[] { notReachableAnnotation, reachableAnnotation };

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            var actual = sut.LotteryDropAnnotation(annotations);

            Assert.That(actual, Is.EqualTo(reachableAnnotation));
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_WithDropAnnotation_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler();
            dropHandler.gameObject.AddComponent<DropAnnotation>();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject,
                new RaycastResult { screenPosition = dragHandler.transform.position });

            Assert.That(dropHandler.WasDrop, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_WithDropTargetObject_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject, dropHandler.gameObject,
                new RaycastResult { screenPosition = dragHandler.transform.position });

            Assert.That(dropHandler.WasDrop, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_WithDropTargetPoint_DropOnTarget()
        {
            var dragHandler = CreateSpyDragHandler();
            var dropHandler = CreateSpyDropHandler();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject, dropHandler.transform.position,
                new RaycastResult { screenPosition = dragHandler.transform.position });

            Assert.That(dropHandler.WasDrop, Is.True);
        }

        [Test]
        [LoadScene("../../Scenes/Canvas.unity")]
        public async Task OperateAsync_WithoutDropTarget_DropOnRandomScreenPoint()
        {
            var dragHandler = CreateSpyDragHandler();

            await UniTask.NextFrame(); // wait ready for raycaster

            var sut = new UguiDragAndDropOperator();
            await sut.OperateAsync(dragHandler.gameObject,
                new RaycastResult { screenPosition = dragHandler.transform.position });

            Assert.That(dragHandler.WasPointerDown, Is.True);
            Assert.That(dragHandler.WasInitializePotentialDrag, Is.True);
            Assert.That(dragHandler.WasBeginDrag, Is.True);
            Assert.That(dragHandler.LastDragPosition, Is.Not.EqualTo(default(Vector2)));
            Assert.That(dragHandler.WasPointerUp, Is.True);
            Assert.That(dragHandler.WasEndDrag, Is.True);
        }
    }
}
