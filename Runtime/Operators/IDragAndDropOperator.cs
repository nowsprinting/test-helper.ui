// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Drag and drop operator interface.
    /// </summary>
    public interface IDragAndDropOperator : IOperator
    {
        /// <summary>
        /// Drag and drop with destination <c>GameObject</c> and drag speed.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="destination">Drop destination <c>GameObject</c></param>
        /// <param name="dragSpeed">Drag speed in units per second. If omitted, use the constructor value.</param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, GameObject destination, int dragSpeed = 0,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Drag and drop with destination point and drag speed.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="destination">Drop destination screen point</param>
        /// <param name="dragSpeed">Drag speed in units per second. If omitted, use the constructor value.</param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, Vector2 destination, int dragSpeed = 0,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default);
    }
}
