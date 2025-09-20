// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Extensions
{
    [TestFixture]
    public class EventTriggerExtensionsTest
    {
        [Test]
        public void CanHandle_NoTrigger_ReturnsFalse()
        {
            var eventTrigger = new GameObject().AddComponent<EventTrigger>();
            Assume.That(eventTrigger.triggers, Is.Empty);

            var actual = eventTrigger.CanHandle<IPointerClickHandler>();
            Assert.That(actual, Is.False);
        }

        [Test]
        public void CanHandle_NoCallback_ReturnsFalse()
        {
            var eventTrigger = new GameObject().AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerClick,
                callback = null
            });
            var actual = eventTrigger.CanHandle<IPointerClickHandler>();
            Assert.That(actual, Is.False);
        }

        private static IEnumerable<TestCaseData> CanHandle_TestCases()
        {
            // @formatter:off
            yield return new TestCaseData(typeof(IPointerEnterHandler), EventTriggerType.PointerEnter).SetName("PointerEnter");
            yield return new TestCaseData(typeof(IPointerExitHandler), EventTriggerType.PointerExit).SetName("PointerExit");
            yield return new TestCaseData(typeof(IPointerDownHandler), EventTriggerType.PointerDown).SetName("PointerDown");
            yield return new TestCaseData(typeof(IPointerUpHandler), EventTriggerType.PointerUp).SetName("PointerUp");
            yield return new TestCaseData(typeof(IPointerClickHandler), EventTriggerType.PointerClick).SetName("PointerClick");
            yield return new TestCaseData(typeof(IDragHandler), EventTriggerType.Drag).SetName("Drag");
            yield return new TestCaseData(typeof(IDropHandler), EventTriggerType.Drop).SetName("Drop");
            yield return new TestCaseData(typeof(IScrollHandler), EventTriggerType.Scroll).SetName("Scroll");
            yield return new TestCaseData(typeof(IUpdateSelectedHandler), EventTriggerType.UpdateSelected).SetName("UpdateSelected");
            yield return new TestCaseData(typeof(ISelectHandler), EventTriggerType.Select).SetName("Select");
            yield return new TestCaseData(typeof(IDeselectHandler), EventTriggerType.Deselect).SetName("Deselect");
            yield return new TestCaseData(typeof(IMoveHandler), EventTriggerType.Move).SetName("Move");
            yield return new TestCaseData(typeof(IInitializePotentialDragHandler),EventTriggerType.InitializePotentialDrag).SetName("InitializePotentialDrag");
            yield return new TestCaseData(typeof(IBeginDragHandler), EventTriggerType.BeginDrag).SetName("BeginDrag");
            yield return new TestCaseData(typeof(IEndDragHandler), EventTriggerType.EndDrag).SetName("EndDrag");
            yield return new TestCaseData(typeof(ISubmitHandler), EventTriggerType.Submit).SetName("Submit");
            yield return new TestCaseData(typeof(ICancelHandler), EventTriggerType.Cancel).SetName("Cancel");
            // @formatter:on
        }

        [TestCaseSource(nameof(CanHandle_TestCases))]
        public void CanHandle_ReturnsTrue(Type handlerType, EventTriggerType eventTriggerType)
        {
            var eventTrigger = new GameObject().AddComponent<EventTrigger>();
            eventTrigger.triggers.Add(new EventTrigger.Entry
            {
                eventID = eventTriggerType,
                callback = new EventTrigger.TriggerEvent()
            });

            var sut = typeof(EventTriggerExtensions).GetMethod("CanHandle",
                BindingFlags.Static | BindingFlags.Public);
            Assume.That(sut, Is.Not.Null);
            var genericMethod = sut.MakeGenericMethod(handlerType);
            var actual = (bool)genericMethod.Invoke(eventTrigger, new object[] { eventTrigger });

            Assert.That(actual, Is.True);
        }
    }
}
