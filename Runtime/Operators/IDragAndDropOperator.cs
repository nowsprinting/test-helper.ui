// Copyright (c) 2023-2025 Koji Hasegawa.
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
        /// Drag and drop with destination <c>GameObject</c>.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="destination">Drop destination <c>GameObject</c>. Drag speed is assumed to be specified in the constructor.</param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, GameObject destination, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Drag and drop with destination point.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="destination">Drop destination point. Drag speed is assumed to be specified in the constructor.</param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, Vector2 destination, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);
    }
}
