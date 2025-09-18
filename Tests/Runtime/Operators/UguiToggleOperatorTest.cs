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
        [CreateScene]
        public void CanOperate_WithToggleComponent_ReturnTrue()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Toggle>();

            Assert.That(_sut.CanOperate(gameObject), Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithButtonComponent_ReturnFalse()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Button>();

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
