// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Visualizers
{
    /// <summary>
    /// Visualizer used for debugging UI tests.
    /// For usage, see <see cref="Monkey"/> and <see cref="GameObjectFinder"/>.
    /// </summary>
    public interface IVisualizer
    {
        /// <summary>
        /// Show the visual indication of the "not reachable" screen point.
        /// </summary>
        /// <param name="screenPoint">Screen point of "not reachable".</param>
        /// <param name="blocker"><c>GameObject</c> that blocked the raycaster.</param>
        void ShowNotReachableIndicator(Vector2 screenPoint, GameObject blocker = null);

        /// <summary>
        /// Show the visual indication of the "not interactable" <c>GameObject</c>.
        /// </summary>
        /// <param name="gameObject"></param>
        void ShowNotInteractableIndicator(GameObject gameObject);

        /// <summary>
        /// Show the visual indication of the "ignored" <c>GameObject</c>.
        /// </summary>
        /// <param name="gameObject"></param>
        void ShowIgnoredIndicator(GameObject gameObject);
    }
}
