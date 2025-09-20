// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiClickAndHoldOperatorTest
    {
        private readonly IOperator _sut = new UguiClickAndHoldOperator(holdMillis: 500);

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
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));
            Object.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));
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
        public void CanOperate_WithEventTriggerOnlyIncludePointerDown_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerDownEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithEventTriggerOnlyIncludePointerUp_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerUpEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithEventTriggerIncludePointerDownAndUp_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerDownEntry());
            eventTrigger.triggers.Add(CreatePointerUpEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledEventTriggerIncludePointerDownAndUp_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerDownEntry());
            eventTrigger.triggers.Add(CreatePointerUpEntry());
            eventTrigger.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
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
        [CreateScene]
        public void CanOperate_WithOnlyPointerDownHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithOnlyPointerUpHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerUpHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithPointerDownAndUpHandler_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledPointerDownHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));
            var handler = gameObject.GetComponent<SpyOnPointerDownHandler>();
            handler.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledPointerUpHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerDownHandler), typeof(SpyOnPointerUpHandler));
            var handler = gameObject.GetComponent<SpyOnPointerUpHandler>();
            handler.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_EventHandler_InvokeOnPointerDownAndUp()
        {
            var gameObject = new GameObject("ClickAndHoldTarget", typeof(SpyOnPointerDownHandler),
                typeof(SpyOnPointerUpHandler));

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerUp");
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_EventTrigger_InvokeOnPointerDownAndUp()
        {
            var gameObject = new GameObject("ClickAndHoldTarget", typeof(SpyPointerDownUpEventReceiver));

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.ReceivePointerUp");
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_DestroyAfterPointerDown_NoError()
        {
            var gameObject = new GameObject("ClickAndHoldTarget", typeof(StubDestroyingItselfWhenPointerDown));

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.Destroy");
        }

        [Test]
        [CreateScene]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task OperateAsync_Cancel()
        {
            var gameObject = new GameObject("ClickAndHoldTarget", typeof(StubLogErrorWhenOnPointerUp));

            Assume.That(_sut.CanOperate(gameObject), Is.True);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                _sut.OperateAsync(gameObject, cancellationToken: cancellationTokenSource.Token).Forget();
                await UniTask.NextFrame();

                cancellationTokenSource.Cancel(); // Not output LogError from StubLogErrorWhenOnPointerUp
                await UniTask.NextFrame();

                LogAssert.Expect(LogType.Log, "ClickAndHoldTarget.OnPointerDown");
            }
        }
    }
}
