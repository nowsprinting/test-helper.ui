// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine.EventSystems;

namespace TestHelper.UI.Extensions
{
    public static class EventTriggerTypeExtensions
    {
        /// <summary>
        /// Returns true if the specified event handler is passive.
        /// A "passive event" is defined as an event that does not initiate a user action and is not interactable.
        /// e.g., <see cref="EventTriggerType.Drop"/>.
        /// </summary>
        /// <seealso cref="IEventSystemHandlerExtensions.HasActiveHandler"/>
        public static bool IsPassive(this EventTriggerType type)
        {
            return type == EventTriggerType.Drop ||
                   type == EventTriggerType.UpdateSelected ||
                   type == EventTriggerType.Deselect;
        }
    }
}
