// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
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
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiToggleOperator(Func<GameObject, Vector2> getScreenPoint = null,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
            : base(getScreenPoint, logger, screenshotOptions)
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
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be clicked.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
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
