// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiClickOperatorTest
    {
        private readonly IOperator _sut = new UguiClickOperator();

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
            var gameObject = new GameObject(null, typeof(SpyOnPointerClickHandler));
            Object.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerClickHandler));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(SpyOnPointerClickHandler));
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
        public void CanOperate_WithEventTriggerNotIncludePointerClick_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(EventTrigger));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithEventTriggerIncludePointerClick_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerClickEntry());

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledEventTriggerIncludePointerClick_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreatePointerClickEntry());
            eventTrigger.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        private static EventTrigger.Entry CreatePointerClickEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick,
                callback = new EventTrigger.TriggerEvent()
            };

        [Test]
        [CreateScene]
        public void CanOperate_WithPointerClickHandler_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerClickHandler));

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisabledPointerClickHandler_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(SpyOnPointerClickHandler));
            var handler = gameObject.GetComponent<SpyOnPointerClickHandler>();
            handler.enabled = false;

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_EventHandler_InvokeOnClick()
        {
            var gameObject = new GameObject("ClickTarget", typeof(SpyOnPointerClickHandler));

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickTarget.OnPointerClick");
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_EventTrigger_InvokeOnClick()
        {
            var gameObject = new GameObject("ClickTarget", typeof(SpyPointerClickEventReceiver));

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickTarget.ReceivePointerClick");
        }
    }
}
