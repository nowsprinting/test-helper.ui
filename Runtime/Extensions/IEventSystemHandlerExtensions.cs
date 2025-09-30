// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
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
            return handler.GetType().GetInterfaces()
                .Where(x => typeof(IEventSystemHandler).IsAssignableFrom(x) && x != typeof(IEventSystemHandler))
                .Any(x =>
                    x != typeof(IDropHandler) &&
                    x != typeof(IUpdateSelectedHandler) &&
                    x != typeof(IDeselectHandler));
        }
    }
}
