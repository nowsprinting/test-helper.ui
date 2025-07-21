// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Scroll wheel of mouse operator interface.
    /// scrolling up/down and tilting left/right.
    /// </summary>
    public interface IScrollWheelOperator : IOperator
    {
        /// <summary>
        /// Scroll with scroll delta.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="destination">Scroll destination point. Scroll speed is assumed to be specified in the constructor.</param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, Vector2 destination, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);
    }
}
