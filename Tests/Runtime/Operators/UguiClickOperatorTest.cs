// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiClickOperatorTest
    {
        private readonly IOperator _sut = new UguiClickOperator();

        [Test]
        public void CanOperate_CanNotClick_ReturnFalse()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<SpyOnPointerDownUpHandler>();

            Assert.That(_sut.CanOperate(gameObject), Is.False);
        }

        [Test]
        [LoadScene("../../Scenes/MissingComponent.unity")]
        public void CanOperate_ButtonWithMissingComponent_ReturnTrue()
        {
            var buttonWithMissing = GameObject.Find("Button with Missing");

            Assert.That(_sut.CanOperate(buttonWithMissing), Is.True);
        }

        [Test]
        public async Task OperateAsync_EventHandler_InvokeOnClick()
        {
            var gameObject = new GameObject("ClickTarget");
            gameObject.AddComponent<SpyOnPointerClickHandler>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickTarget.OnPointerClick");
        }

        [Test]
        public async Task OperateAsync_EventTrigger_InvokeOnClick()
        {
            var gameObject = new GameObject("ClickTarget");
            gameObject.AddComponent<SpyPointerClickEventReceiver>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            await _sut.OperateAsync(gameObject);

            LogAssert.Expect(LogType.Log, "ClickTarget.ReceivePointerClick");
        }
    }
}
