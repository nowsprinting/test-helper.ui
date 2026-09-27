// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Operator that performs one kind of operation, such as click, on a <c>GameObject</c> for monkey testing.
    /// Implement the <c>CanOperate</c> method to determine whether an operation such as click is possible, and the <c>OperateAsync</c> method to execute the operation.
    /// </summary>
    /// <remarks>
    /// Implement a sub-interface (e.g., <c>IClickOperator</c>) to represent the kind of operation.
    /// If required parameters for the operation, such as hold time, input text strategy, etc., keep them in instance fields of the implementation class.
    /// <p/>
    /// <c>OperatorPool</c> creates instances of the implementation class by invoking its public constructor via reflection.
    /// Annotate every public constructor with <c>[UnityEngine.Scripting.Preserve]</c> so that managed code stripping does not remove it from the Player build;
    /// for the implicit default constructor, annotate the class instead.
    /// <c>OperatorPool</c> cannot invoke a non-public constructor, even with registered constructor arguments.
    /// When registered without constructor arguments, the implementation class must have exactly one public constructor,
    /// and each of its parameters must have a default value or a type whose value is passed to the <c>OperatorPool</c> constructor.
    /// If the implementation class has multiple public constructors or a parameter that cannot be resolved this way,
    /// register it with explicit constructor arguments via <c>OperatorPool.Register&lt;T&gt;(args)</c>.
    /// </remarks>
    public interface IOperator
    {
        /// <summary>
        /// Logger set if you need.
        /// </summary>
        ILogger Logger { set; }

        /// <summary>
        /// Take screenshot options set if you need.
        /// </summary>
        ScreenshotOptions ScreenshotOptions { set; }

        /// <summary>
        /// Visualizer for the operation; set it if you need.
        /// </summary>
        IVisualizer Visualizer { set; }

        /// <summary>
        /// Returns if can operate target <c>GameObject</c> this Operator.
        /// <p/>
        /// Note: Does not check if reachable by user. 
        /// </summary>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <returns>True if can operate <c>GameObject</c> this Operator.</returns>
        bool CanOperate(GameObject gameObject);

        /// <summary>
        /// Execute this operator in monkey testing.
        /// </summary>
        /// <remarks>
        /// If required parameters for the operation, such as hold time, input text strategy, etc., keep them in instance fields of the implementation class.
        /// If you want to add parameters for execution outside of monkey tests, define a method in the sub-interface (e.g., <c>ITextInputOperator</c>).
        /// </remarks>
        /// <param name="gameObject">Operation target <c>GameObject</c></param>
        /// <param name="raycastResult">Includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the operator implementation.</param>
        /// <param name="cancellationToken">Cancellation token for operation (e.g., click and hold)</param>
        UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default);
    }
}
