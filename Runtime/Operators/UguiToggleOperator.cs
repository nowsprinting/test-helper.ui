// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Toggle operator for <see cref="Toggle"/> component.
    /// You can click to turn it on/off, or you can specify the on/off state.
    /// </summary>
    /// <remarks>
    /// If state is not specified (e.g., in monkey testing), it will always be flipped (same as click).
    /// </remarks>
    /// <seealso cref="UguiClickOperator"/>
    public class UguiToggleOperator : UguiClickOperator, IToggleOperator
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiToggleOperator(ILogger logger = null, ScreenshotOptions screenshotOptions = null)
            : base(logger, screenshotOptions)
        {
        }

        /// <inheritdoc />
        /// <remarks>The <c>new</c> keyword is specified because we want it to work with the casted type.</remarks>
        public new bool CanOperate(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            return gameObject.TryGetEnabledComponent<Toggle>(out _);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Does not click if the toggle state is already specified.
        /// <p/>
        /// This operator receives <c>RaycastResult</c>, but passing <c>default</c> may be OK, depending on the component being operated on.
        /// </remarks>
        public UniTask OperateAsync(GameObject gameObject, bool isOn, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            var toggle = gameObject.GetComponent<Toggle>();
            if (toggle.isOn == isOn)
            {
                return UniTask.CompletedTask;
            }

            return base.OperateAsync(gameObject, raycastResult, cancellationToken);
        }
    }
}
