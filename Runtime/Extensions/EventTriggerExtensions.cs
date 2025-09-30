// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Extensions
{
    public static class EventTriggerExtensions
    {
        /// <summary>
        /// Returns true if the specified event handler can be handled.
        /// <p/>
        /// Note: This method does not check if the GameObject is active or if the EventTrigger component is enabled.
        /// </summary>
        public static bool CanHandle<T>(this EventTrigger eventTrigger) where T : IEventSystemHandler
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Object when typeof(T) == typeof(IPointerEnterHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.PointerEnter);
                case TypeCode.Object when typeof(T) == typeof(IPointerExitHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.PointerExit);
                case TypeCode.Object when typeof(T) == typeof(IPointerDownHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.PointerDown);
                case TypeCode.Object when typeof(T) == typeof(IPointerUpHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.PointerUp);
                case TypeCode.Object when typeof(T) == typeof(IPointerClickHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.PointerClick);
                case TypeCode.Object when typeof(T) == typeof(IDragHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Drag);
                case TypeCode.Object when typeof(T) == typeof(IDropHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Drop);
                case TypeCode.Object when typeof(T) == typeof(IScrollHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Scroll);
                case TypeCode.Object when typeof(T) == typeof(IUpdateSelectedHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.UpdateSelected);
                case TypeCode.Object when typeof(T) == typeof(ISelectHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Select);
                case TypeCode.Object when typeof(T) == typeof(IDeselectHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Deselect);
                case TypeCode.Object when typeof(T) == typeof(IMoveHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Move);
                case TypeCode.Object when typeof(T) == typeof(IInitializePotentialDragHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.InitializePotentialDrag);
                case TypeCode.Object when typeof(T) == typeof(IBeginDragHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.BeginDrag);
                case TypeCode.Object when typeof(T) == typeof(IEndDragHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.EndDrag);
                case TypeCode.Object when typeof(T) == typeof(ISubmitHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Submit);
                case TypeCode.Object when typeof(T) == typeof(ICancelHandler):
                    return eventTrigger.CanExecuteEventTriggerType(EventTriggerType.Cancel);
            }

            return false;
        }

        private static bool CanExecuteEventTriggerType(this EventTrigger eventTrigger, EventTriggerType type)
        {
            return eventTrigger.triggers.Any(x => x.eventID == type && x.callback != null);
        }

        /// <summary>
        /// Returns true if this <c>EventTrigger</c> has any active (non-passive) event trigger entries.
        /// A "passive event" is defined as an event that does not initiate a user action and is not interactable.
        /// e.g., <see cref="EventTriggerType.Drop"/>.
        /// <p/>
        /// Note: This method does not check if the GameObject is active or if the EventTrigger component is enabled.
        /// </summary>
        /// <see cref="EventTriggerTypeExtensions.IsPassive"/>
        /// <seealso cref="IEventSystemHandlerExtensions.HasActiveHandler"/>
        public static bool HasActiveTrigger(this EventTrigger eventTrigger)
        {
            return eventTrigger.triggers.Any(x => !x.eventID.IsPassive() && x.callback != null);
        }
    }
}
