// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Swipe operator (including flick) interface.
    /// </summary>
    /// <remarks>
    /// If a flick operation is required, increasing the swipe speed parameter will determine it as a flick.
    /// </remarks>
    public interface ISwipeOperator : IOperator
    {
        /// <summary>
        /// Swipe with direction and swipe speed.
        /// </summary>
        /// <param name="gameObject">Target GameObject to operate.</param>
        /// <param name="direction">Swipe direction (will be normalized).</param>
        /// <param name="swipeSpeed">Swipe speed in units per second. If omitted, use the constructor value.</param>
        /// <param name="raycastResult">RaycastResult from the cursor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task to await.</returns>
        UniTask OperateAsync(GameObject gameObject, Vector2 direction, int swipeSpeed = 0,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default);
    }
}
