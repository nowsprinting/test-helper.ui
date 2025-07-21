// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestHelper.UI.Operators
{
    [TestFixture]
    public class UguiTextInputOperatorTest
    {
        private ITextInputOperator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new UguiTextInputOperator(randomString: new StubRandomString("RANDOM"));
        }

        [Test]
        public void CanOperate_NotInputText_ReturnFalse()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<Button>();

            Assert.That(_sut.CanOperate(gameObject), Is.False);
        }

        [Test]
        [LoadScene("../../Scenes/MissingComponent.unity")]
        public void CanOperate_InputFieldWithMissingComponent_ReturnTrue()
        {
            var inputFieldWithMissing = GameObject.Find("InputField with Missing");

            Assert.That(_sut.CanOperate(inputFieldWithMissing), Is.True);
        }

        [Test]
        public void OperateAsync_InputText_SetsRandomText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject);

            Assert.That(inputField.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_InputTextWithText_SetsSpecifiedText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject, "SPECIFIED");

            Assert.That(inputField.text, Is.EqualTo("SPECIFIED"));
        }

        [Test]
        public void OperateAsync_TmpInputText_SetsRandomText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<TMP_InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject);

            Assert.That(inputField.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_TmpInputTextWithText_SetsSpecifiedText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<TMP_InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject, "SPECIFIED");

            Assert.That(inputField.text, Is.EqualTo("SPECIFIED"));
        }
    }
}
