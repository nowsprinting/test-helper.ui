// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Extensions
{
    /// <summary>
    /// A utility to select Camera
    /// </summary>
    public static class GameObjectExtensions
    {
        private static Camera s_cachedMainCamera;
        private static int s_cachedFrame;

        /// <summary>
        /// Returns the first enabled Camera component that is tagged "MainCamera"
        /// </summary>
        /// <returns>The first enabled Camera component that is tagged "MainCamera"</returns>
        private static Camera GetMainCamera()
        {
            if (Time.frameCount == s_cachedFrame)
            {
                return s_cachedMainCamera;
            }

            s_cachedFrame = Time.frameCount;
            return s_cachedMainCamera = Camera.main;
        }

        /// <summary>
        /// Returns an associated camera with <paramref name="gameObject"/>.
        /// Or return <c cref="Camera.main">Camera.main</c> if there are no camera associated with.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns>
        /// Camera associated with <paramref name="gameObject"/>, or return <c cref="Camera.main">Camera.main</c> if there are no camera associated with
        /// </returns>
        public static Camera GetAssociatedCamera(this GameObject gameObject)
        {
            var canvas = gameObject.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return GetMainCamera();
            }

            if (!canvas.isRootCanvas)
            {
                canvas = canvas.rootCanvas;
            }

            return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        }

        /// <summary>
        /// Return interactable components.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="isComponentInteractable"></param>
        /// <returns></returns>
        public static IEnumerable<Component> GetInteractableComponents(this GameObject gameObject,
            Func<Component, bool> isComponentInteractable = null)
        {
            isComponentInteractable = isComponentInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;

            return gameObject.GetComponents<Component>().Where(x => isComponentInteractable.Invoke(x));
        }

        /// <summary>
        /// Try to get a component exclude disabled component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="component">A component of the matching type, if found.</param>
        /// <typeparam name="T">The type of component to search for</typeparam>
        /// <returns>True if found event handler, active, and enabled</returns>
        public static bool TryGetEnabledComponent<T>(this GameObject gameObject, out T component)
        {
            component = gameObject.GetComponent<T>();
            return component != null && (!(component is Behaviour) || (component as Behaviour).isActiveAndEnabled);
        }

        /// <summary>
        /// Get components that can handle the specified event handler.
        /// If <see cref="EventTrigger"/>, only those with a matching <see cref="EventTrigger.Entry"/> type is returned.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T">The type of component to search for</typeparam>
        /// <returns>Event handlers, active, and enabled</returns>
        /// <seealso cref="EventTriggerExtensions.CanHandle{T}"/>
        public static IEnumerable<MonoBehaviour> GetEventHandlers<T>(this GameObject gameObject)
            where T : IEventSystemHandler
        {
            foreach (var component in gameObject.GetComponents<MonoBehaviour>().Where(x => x.isActiveAndEnabled))
            {
                if (component is EventTrigger eventTrigger)
                {
                    if (eventTrigger.CanHandle<T>())
                    {
                        yield return component;
                    }
                }
                else if (component is T)
                {
                    yield return component;
                }
            }
        }

        /// <summary>
        /// Check whether a GameObject can handle the specified event handler.
        /// If <see cref="EventTrigger"/>, only those with a matching <see cref="EventTrigger.Entry"/> type are valid.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <typeparam name="T">The type of component to search for</typeparam>
        /// <returns>True if found event handler, active, and enabled</returns>
        /// <seealso cref="EventTriggerExtensions.CanHandle{T}"/>
        public static bool HasEventHandlers<T>(this GameObject gameObject) where T : IEventSystemHandler
        {
            foreach (var component in gameObject.GetComponents<MonoBehaviour>().Where(x => x.isActiveAndEnabled))
            {
                if (component is EventTrigger eventTrigger)
                {
                    if (eventTrigger.CanHandle<T>())
                    {
                        return true;
                    }
                }
                else if (component is T)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns the operators available to this component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Available operators</returns>
        public static IEnumerable<IOperator> SelectOperators(this GameObject gameObject,
            IEnumerable<IOperator> operators)
        {
            return operators.Where(iOperator => iOperator.CanOperate(gameObject));
        }

        /// <summary>
        /// Returns the operators that specify types and are available to this component.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        /// <returns>Available operators</returns>
        public static IEnumerable<T> SelectOperators<T>(this GameObject gameObject, IEnumerable<IOperator> operators)
            where T : IOperator
        {
            return operators.OfType<T>().Where(iOperator => iOperator.CanOperate(gameObject));
        }
    }
}
