// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using UnityEngine.UI;
// UnityEngine.TestTools.Constraints is imported for the AllocatingGCMemory extension method, which brings a
// second `Is` into scope. Aliased to NUnit's so that the existing assertions in this file keep resolving to it.
using Is = NUnit.Framework.Is;

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
        public void TryGetEnabledComponent_ComponentMissing_DoesNotAllocateGCMemory()
        {
            var gameObject = new GameObject("Image", typeof(Image));
            Assume.That(gameObject.TryGetEnabledComponent<Button>(out _), Is.False); // also warms up the measured path

            // Not a lambda with an expression body: a value-returning one binds to the ActualValueDelegate<T>
            // overload of Assert.That, and the constraint then rejects it as "not a TestDelegate".
            Assert.That(() => { _ = gameObject.TryGetEnabledComponent<Button>(out _); },
                Is.Not.AllocatingGCMemory());
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
        [CreateScene]
        public void TryGetEnabledComponentInParent_NotActive_ReturnsFalse()
        {
            var gameObject = new GameObject("Button", typeof(Button));
            gameObject.SetActive(false);

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
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
        [CreateScene]
        public void TryGetEnabledComponentInParent_NotEnabled_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var button = gameObject.AddComponent<Button>();
            button.enabled = false;

            var actual = gameObject.TryGetEnabledComponentInParent<Button>(out var _);
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
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

        [Test]
        [CreateScene]
        public void TryGetEnabledComponentInParent_ComponentMissing_DoesNotAllocateGCMemory()
        {
            var parent = new GameObject("Parent", typeof(Image));
            var gameObject = new GameObject("Child", typeof(Image));
            gameObject.transform.SetParent(parent.transform);
            Assume.That(gameObject.TryGetEnabledComponentInParent<Button>(out _),
                Is.False); // also warms up the measured path

            Assert.That(() => { _ = gameObject.TryGetEnabledComponentInParent<Button>(out _); },
                Is.Not.AllocatingGCMemory());
        }
    }
}
