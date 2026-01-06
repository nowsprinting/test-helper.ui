// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Click and hold operator interface.
    /// a.k.a. touch and hold, long press.
    /// </summary>
    public interface IClickAndHoldOperator : IOperator
    {
        /// <summary>
        /// Click and hold with specified hold time.
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="holdMillis">Hold time in milliseconds. If 0 or less, use constructor value.</param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation</param>
        UniTask OperateAsync(GameObject gameObject, int holdMillis, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);
    }
}
