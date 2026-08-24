// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

namespace TestHelper.UI.Extensions
{
    public static class IEventSystemHandlerExtensions
    {
        /// <summary>
        /// Returns true if this event handler implements one or more active (non-passive) event handler.
        /// A "passive event" is defined as an event that does not initiate a user action and is not interactable.
        /// e.g., <see cref="IDropHandler"/>.
        /// </summary>
        /// <seealso cref="EventTriggerExtensions.HasActiveTrigger"/>
        public static bool HasActiveHandler(this IEventSystemHandler handler)
        {
            foreach (var type in handler.GetType().GetInterfaces())
            {
                if (!typeof(IEventSystemHandler).IsAssignableFrom(type) || type == typeof(IEventSystemHandler))
                {
                    continue;
                }

                if (type != typeof(IDropHandler) &&
                    type != typeof(IUpdateSelectedHandler) &&
                    type != typeof(IDeselectHandler))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
