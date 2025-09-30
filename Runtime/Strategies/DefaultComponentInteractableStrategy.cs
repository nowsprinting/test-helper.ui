// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Strategies
{
    /// <summary>
    /// Default strategy to examine whether Component is interactable.
    /// </summary>
    public static class DefaultComponentInteractableStrategy
    {
        /// <summary>
        /// Returns true if this <c>Component</c> is interactable.
        /// It is considered interactable when the following conditions are met:
        /// <list type="number">
        ///   <item><c>Component</c> is not null, active, and enabled</item>
        ///   <item>If the <c>Component</c> type is <see cref="Selectable"/>, the <c>interactable</c> property is true</item>
        ///   <item>If the <c>Component</c> type is <see cref="EventTrigger"/>, has one or more non-passive triggers</item>
        ///   <item>If the <c>Component</c> type implements <see cref="IEventSystemHandler"/>, contains one or more non-passive events</item>
        /// </list>
        /// <p/>
        /// Note: Does not check if reachable by user. 
        /// </summary>
        /// <see cref="EventTriggerExtensions.HasActiveTrigger"/>
        /// <see cref="IEventSystemHandlerExtensions.HasActiveHandler"/>
        public static bool IsInteractable(Component component)
        {
            if (component == null || (component is Behaviour behaviour && !behaviour.isActiveAndEnabled))
            {
                return false;
            }

            if (component is Selectable selectable)
            {
                return selectable.interactable;
            }

            if (component is EventTrigger eventTrigger)
            {
                return eventTrigger.HasActiveTrigger();
            }

            if (component is IEventSystemHandler eventHandler)
            {
                return eventHandler.HasActiveHandler();
            }

            return false;
        }
    }
}
