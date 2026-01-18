// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Scroll wheel of mouse operator interface.
    /// scrolling up/down and tilting left/right.
    /// </summary>
    public interface IScrollWheelOperator : IOperator
    {
        /// <summary>
        /// Scroll with direction and distance.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="direction">The normalized direction vector for scrolling</param>
        /// <param name="distance">The distance to scroll (must be positive)</param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, Vector2 direction, int distance,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Scroll with direction, distance, and scroll speed.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="direction">The normalized direction vector for scrolling</param>
        /// <param name="distance">The distance to scroll (must be positive)</param>
        /// <param name="scrollSpeed">Scroll speed in units per second. If 0 or less, use constructor value.</param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, Vector2 direction, int distance, int scrollSpeed,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Scroll with destination screen point.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="destination">Scroll destination screen point. Scroll speed is assumed to be specified in the constructor.</param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        [Obsolete("Use OperateAsync with direction and distance parameters instead.")]
        UniTask OperateAsync(GameObject gameObject, Vector2 destination,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default);

    }
}
