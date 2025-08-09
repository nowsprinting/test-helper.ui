// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TestHelper.UI
{
    /// <summary>
    /// Find interactable components on the scene.
    /// </summary>
    public class InteractableComponentsFinder
    {
        private readonly Func<Component, bool> _isInteractable;
        private readonly IEnumerable<IOperator> _operators;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isInteractable">The function returns the <c>Component</c> is interactable or not. Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="operators">All available operators in autopilot/tests. Usually defined in <c>MonkeyConfig</c></param>
        public InteractableComponentsFinder(
            Func<Component, bool> isInteractable = null,
            IEnumerable<IOperator> operators = null)
        {
            _isInteractable = isInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
            _operators = operators ?? Array.Empty<IOperator>();
        }

        /// <summary>
        /// Find components attached <see cref="EventTrigger"/> or implements <see cref="IEventSystemHandler"/> in the scene.
        /// Includes UI elements that inherit from the <see cref="Selectable"/> class, such as the <see cref="Button"/>.
        /// <p/>
        /// Note: If you only need uGUI elements, use <see cref="UnityEngine.UI.Selectable.allSelectablesArray"/> is faster.
        /// <br/>
        /// Note: Does not check if reachable by user. 
        /// </summary>
        /// <returns>Interactable components</returns>
        public IEnumerable<MonoBehaviour> FindInteractableComponents()
        {
            foreach (var component in FindMonoBehaviours())
            {
                if (_isInteractable.Invoke(component))
                {
                    yield return component;
                }
            }
        }

        /// <summary>
        /// Returns tuple of interactable component and operator.
        /// <p/>
        /// Note: Does not check if reachable by user. 
        /// </summary>
        /// <returns>Tuple of interactable component and operator</returns>
        public IEnumerable<(MonoBehaviour, IOperator)> FindInteractableComponentsAndOperators()
        {
            return FindInteractableComponents()
                .SelectMany(x => x.gameObject.SelectOperators(_operators), (x, o) => (x, o));
        }

        /// <summary>
        /// Find components that can handle the specified event handler in the scene.
        /// <p/>
        /// Note: Does not check if reachable by user.
        /// </summary>
        /// <typeparam name="T">Event handler type</typeparam>
        /// <returns>Components can handle the specified event handler</returns>
        public IEnumerable<MonoBehaviour> FindEventHandlers<T>() where T : IEventSystemHandler
        {
            foreach (var component in FindMonoBehaviours().Where(x => x.isActiveAndEnabled))
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

        private static IEnumerable<MonoBehaviour> FindMonoBehaviours()
        {
#if UNITY_2022_3_OR_NEWER
            return Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return Object.FindObjectsOfType<MonoBehaviour>();
#endif
        }
    }
}
