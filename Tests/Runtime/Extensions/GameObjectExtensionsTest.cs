// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
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
        public void GetInteractableComponents_NoInteractableComponents_ReturnsEmpty()
        {
            var button = new GameObject().AddComponent<Button>();
            button.interactable = false;

            var actual = button.gameObject.GetInteractableComponents();
            Assert.That(actual, Is.Empty);
        }

        [Test]
        public void TryGetEnabledComponent_Null_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        public void TryGetEnabledComponent_NotBehaviour_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var actual = gameObject.TryGetEnabledComponent<Transform>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        public void TryGetEnabledComponent_ActiveAndEnabled_ReturnsTrue()
        {
            var gameObject = new GameObject("Button", typeof(Button));
            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.True);
        }

        [Test]
        public void TryGetEnabledComponent_NotActive_ReturnsFalse()
        {
            var gameObject = new GameObject("Button", typeof(Button));
            gameObject.SetActive(false);

            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
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
        public void TryGetEnabledComponent_NotEnabled_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var button = gameObject.AddComponent<Button>();
            button.enabled = false;

            var actual = gameObject.TryGetEnabledComponent<Button>(out var _);
            Assert.That(actual, Is.False);
        }
    }
}
