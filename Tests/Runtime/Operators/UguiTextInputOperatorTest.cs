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
        public void CanOperate_NullGameObject_ReturnsFalse()
        {
            var actual = _sut.CanOperate(null);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_DestroyedGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(InputField));
            Object.DestroyImmediate(gameObject);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(InputField));
            gameObject.SetActive(false);

            var actual = _sut.CanOperate(gameObject);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void CanOperate_InactiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject(null, typeof(InputField));
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
        public void CanOperate_WithInputFieldComponent_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(InputField));
            Assert.That(_sut.CanOperate(gameObject), Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithTmpInputFieldComponent_ReturnsTrue()
        {
            var gameObject = new GameObject(null, typeof(TMP_InputField));
            Assert.That(_sut.CanOperate(gameObject), Is.True);
        }

        [Test]
        [CreateScene]
        public void CanOperate_WithDisableInputFieldComponent_ReturnsFalse()
        {
            var gameObject = new GameObject(null, typeof(InputField));
            var toggle = gameObject.GetComponent<InputField>();
            toggle.enabled = false;

            Assert.That(_sut.CanOperate(gameObject), Is.False);
        }

        [Test]
        public void OperateAsync_InputField_SetsRandomText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject);

            Assert.That(inputField.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_InputFieldWithText_SetsSpecifiedText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject, "SPECIFIED");

            Assert.That(inputField.text, Is.EqualTo("SPECIFIED"));
        }

        [Test]
        public void OperateAsync_TmpInputField_SetsRandomText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<TMP_InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject);

            Assert.That(inputField.text, Is.EqualTo("RANDOM"));
        }

        [Test]
        public void OperateAsync_TmpInputFieldWithText_SetsSpecifiedText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<TMP_InputField>();

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject, "SPECIFIED");

            Assert.That(inputField.text, Is.EqualTo("SPECIFIED"));
        }

        [Test]
        public void OperateAsync_TmpInputFieldWithValidator_SetsValidatedText()
        {
            var gameObject = new GameObject();
            var inputField = gameObject.AddComponent<TMP_InputField>();
            inputField.onValidateInput += (string text, int index, char addedChar) =>
                addedChar != 'N' ? addedChar : '\0';

            Assume.That(_sut.CanOperate(gameObject), Is.True);
            _sut.OperateAsync(gameObject);

            Assert.That(inputField.text, Is.EqualTo("RADOM"));
        }
    }
}
