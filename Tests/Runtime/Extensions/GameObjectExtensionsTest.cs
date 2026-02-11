// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_2023_1_OR_NEWER
using System.Linq;
using Cysharp.Threading.Tasks;
#endif

namespace TestHelper.UI.Extensions
{
    [TestFixture]
    public class GameObjectExtensionsTest
    {
        [Test]
        [CreateScene]
        public void GetInteractableComponents_GotInteractableComponents()
        {
            var gameObject = new GameObject();
            var onPointerClickHandler = gameObject.AddComponent<SpyOnPointerClickHandler>();
            var onPointerDownHandler = gameObject.AddComponent<SpyOnPointerDownHandler>();
            var onPointerUpHandler = gameObject.AddComponent<SpyOnPointerUpHandler>();
            gameObject.AddComponent<Image>(); // Not interactable

            var actual = gameObject.GetInteractableComponents();
            Assert.That(actual, Is.EquivalentTo(
                new Component[] { onPointerClickHandler, onPointerDownHandler, onPointerUpHandler }));
        }

        [Test]
        [CreateScene]
        public void GetInteractableComponents_NoInteractableComponents_ReturnsEmpty()
        {
            var button = new GameObject().AddComponent<Button>();
            button.interactable = false;

            var actual = button.gameObject.GetInteractableComponents();
            Assert.That(actual, Is.Empty);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponent_Null_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponent_NotBehaviour_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var actual = gameObject.TryGetEnabledComponent<Transform>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponent_ActiveAndEnabled_ReturnsTrue()
        {
            var gameObject = new GameObject("Button", typeof(Button));
            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponent_NotActive_ReturnsFalse()
        {
            var gameObject = new GameObject("Button", typeof(Button));
            gameObject.SetActive(false);

            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponent_NotActiveParent_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject("Button", typeof(Button));
            gameObject.transform.SetParent(parent.transform);
            parent.SetActive(false);

            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponent_NotEnabled_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var button = gameObject.AddComponent<Button>();
            button.enabled = false;

            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponentInParent_Null_ReturnsFalse()
        {
            var gameObject = new GameObject();

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponentInParent_NotBehaviour_ReturnsTrue()
        {
            var gameObject = new GameObject();

            var actual = gameObject.TryGetEnabledComponentInParent<Transform>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponentInParent_ActiveAndEnabled_ReturnsTrue()
        {
            var gameObject = new GameObject("Button", typeof(Button));

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void TryGetEnabledComponentInParent_FoundInParent_ReturnsTrue()
        {
            var parent = new GameObject("Parent", typeof(Button));
            var gameObject = new GameObject("Child");
            gameObject.transform.SetParent(parent.transform);

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        public void TryGetEnabledComponentInParent_NotActive_ReturnsFalse()
        {
            var gameObject = new GameObject("Button", typeof(Button));
            gameObject.SetActive(false);

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void TryGetEnabledComponentInParent_NotActiveParent_ReturnsFalse()
        {
            var parent = new GameObject("Parent", typeof(Button));
            var gameObject = new GameObject("Child");
            gameObject.transform.SetParent(parent.transform);
            parent.SetActive(false);

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void TryGetEnabledComponentInParent_NotEnabled_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var button = gameObject.AddComponent<Button>();
            button.enabled = false;

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void TryGetEnabledComponentInParent_NotEnabledInParent_ReturnsFalse()
        {
            var parent = new GameObject("Parent");
            var button = parent.AddComponent<Button>();
            button.enabled = false;
            var gameObject = new GameObject("Child");
            gameObject.transform.SetParent(parent.transform);

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }
    }
}
