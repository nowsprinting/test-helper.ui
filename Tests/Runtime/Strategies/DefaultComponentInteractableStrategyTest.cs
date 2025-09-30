// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TestHelper.UI.Strategies
{
    [TestFixture]
    public class DefaultComponentInteractableStrategyTest
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var newScene = SceneManager.CreateScene(nameof(DefaultComponentInteractableStrategyTest));
            SceneManager.SetActiveScene(newScene);
        }

        [Test]
        [CreateScene]
        public void IsInteractable_Null_ReturnsFalse()
        {
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(null), Is.False);
        }

        [Test]
        public void IsInteractable_InActiveGameObject_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<SpyOnPointerClickHandler>();
            gameObject.SetActive(false);
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(component), Is.False);
        }

        [Test]
        public void IsInteractable_InActiveParentGameObject_ReturnsFalse()
        {
            var parent = new GameObject();
            var gameObject = new GameObject();
            var component = gameObject.AddComponent<SpyOnPointerClickHandler>();
            gameObject.transform.SetParent(parent.transform);
            parent.SetActive(false);
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(component), Is.False);
        }

        [Test]
        public void IsInteractable_NotInteractableComponent_ReturnsFalse()
        {
            var component = new GameObject().AddComponent<MeshCollider>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(component), Is.False);
        }

        [Test]
        public void IsInteractable_SelectableAndInteractable_ReturnsTrue()
        {
            var selectable = new GameObject().AddComponent<Button>();
            selectable.interactable = true;
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(selectable), Is.True);
        }

        [Test]
        public void IsInteractable_SelectableAndNotInteractable_ReturnsFalse()
        {
            var selectable = new GameObject().AddComponent<Button>();
            selectable.interactable = false;
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(selectable), Is.False);
        }

        [Test]
        public void IsInteractable_EventTriggerHasActiveTriggerType_ReturnsTrue()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateDragEntry());
            eventTrigger.triggers.Add(CreateDropEntry());
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(eventTrigger), Is.True);
        }

        [Test]
        public void IsInteractable_EventTriggerHasOnlyPassiveTrigger_ReturnsFalse()
        {
            var gameObject = new GameObject();
            var eventTrigger = gameObject.AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(CreateDropEntry());
            eventTrigger.enabled = false;
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(eventTrigger), Is.False);
        }

        private static EventTrigger.Entry CreateDragEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drag,
                callback = new EventTrigger.TriggerEvent()
            };

        private static EventTrigger.Entry CreateDropEntry() =>
            new EventTrigger.Entry
            {
                eventID = EventTriggerType.Drop,
                callback = new EventTrigger.TriggerEvent()
            };

        [Test]
        public void IsInteractable_ImplementsActiveEventHandler_ReturnsTrue()
        {
            var handler = new GameObject().AddComponent<FakeDragHandler>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(handler), Is.True);
        }

        private class FakeDragHandler : MonoBehaviour, IDragHandler
        {
            public void OnDrag(PointerEventData eventData) { }
        }

        [Test]
        public void IsInteractable_ImplementsActiveAndPassiveEventHandler_ReturnsTrue()
        {
            var handler = new GameObject().AddComponent<FakeDragAndDropHandler>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(handler), Is.True);
        }

        private class FakeDragAndDropHandler : MonoBehaviour, IDragHandler, IDropHandler
        {
            public void OnDrag(PointerEventData eventData) { }
            public void OnDrop(PointerEventData eventData) { }
        }

        [Test]
        public void IsInteractable_ImplementsOnlyPassiveEvetHandler_ReturnsFalse()
        {
            var handler = new GameObject().AddComponent<FakeDropHandler>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(handler), Is.False);
        }

        private class FakeDropHandler : MonoBehaviour, IDropHandler
        {
            public void OnDrop(PointerEventData eventData) { }
        }

        [Test]
        public void IsInteractable_ImplementsOnlyMultiplePassiveEvetHandler_ReturnsFalse()
        {
            var handler = new GameObject().AddComponent<FakeUpdateAndDeselectHandler>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(handler), Is.False);
        }

        private class FakeUpdateAndDeselectHandler : MonoBehaviour, IUpdateSelectedHandler, IDeselectHandler
        {
            public void OnUpdateSelected(BaseEventData eventData) { }
            public void OnDeselect(BaseEventData eventData) { }
        }

        [Test]
        public void IsInteractable_ImplementsPassiveEvetHandlerAndNotHandler_ReturnsFalse()
        {
            var handler = new GameObject().AddComponent<FakeDropHandlerAndDisposable>();
            Assert.That(DefaultComponentInteractableStrategy.IsInteractable(handler), Is.False);
        }

        private sealed class FakeDropHandlerAndDisposable : MonoBehaviour, IDropHandler, IDisposable
        {
            public void OnDrop(PointerEventData eventData) { }
            public void Dispose() { }
        }
    }
}
