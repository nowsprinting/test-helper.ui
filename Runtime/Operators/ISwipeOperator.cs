// Copyright (c) 2023-2025 Koji Hasegawa.
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
        /// Perform swipe operation with specified direction.
        /// </summary>
        /// <param name="gameObject">Target GameObject to operate.</param>
        /// <param name="direction">Swipe direction (will be normalized).</param>
        /// <param name="raycastResult">RaycastResult from the cursor.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task to await.</returns>
        UniTask OperateAsync(GameObject gameObject, Vector2 direction, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);
    }
}
