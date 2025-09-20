// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiToggleOperatorTest
    {
        private readonly IToggleOperator _sut = new UguiToggleOperator();

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
            var gameObject = new GameObject(null, typeof(Toggle));
            Object.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(Toggle));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(Toggle));
            gameObject.transform.SetParent(parent.transform);
            parent.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithButtonComponent_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(Button));
            Assert.That(_sut.CanOperate(gameObject), Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithToggleComponent_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(Toggle));
            Assert.That(_sut.CanOperate(gameObject), Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisableToggleComponent_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(Toggle));
            var toggle = gameObject.GetComponent<Toggle>();
            toggle.enabled = false;

            Assert.That(_sut.CanOperate(gameObject), Is.False);
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_WithIsOn_AlwaysSpecifiedState(
            [Values] bool beforeState,
            [Values] bool specifyState)
        {
            var gameObject = new GameObject();
            var toggle = gameObject.AddComponent<Toggle>();
            toggle.isOn = beforeState;

            await _sut.OperateAsync(gameObject, specifyState);

            Assert.That(toggle.isOn, Is.EqualTo(specifyState));
        }

        [Test]
        [CreateScene]
        public async Task OperateAsync_WithoutIsOn_FlipState([Values] bool state)
        {
            var gameObject = new GameObject();
            var toggle = gameObject.AddComponent<Toggle>();
            toggle.isOn = state;

            await _sut.OperateAsync(gameObject);

            Assert.That(toggle.isOn, Is.Not.EqualTo(state)); // flip
        }

        [Test]
        [CreateScene]
        public async Task Constructor_WithLogger_UseLogger()
        {
            var spyLogger = new SpyLogger();
            var sut = new UguiToggleOperator(spyLogger);
            var gameObject = new GameObject();
            gameObject.AddComponent<Toggle>();

            await sut.OperateAsync(gameObject);

            Assert.That(spyLogger.Messages[0], Does.StartWith("UguiToggleOperator operates to"));
        }
    }
}
