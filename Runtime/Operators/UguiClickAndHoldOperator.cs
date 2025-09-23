// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Click and hold operator for Unity UI (uGUI) components.
    /// a.k.a. touch and hold, long press.
    /// </summary>
    public class UguiClickAndHoldOperator : IClickAndHoldOperator, IScreenPointCustomizable
    {
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        /// <inheritdoc/>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; }

        private readonly int _holdMillis;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="holdMillis">Hold time in milliseconds</param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiClickAndHoldOperator(int holdMillis = 1000, Func<GameObject, Vector2> getScreenPoint = null,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            _holdMillis = holdMillis;
            GetScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            Logger = logger ?? Debug.unityLogger;
            ScreenshotOptions = screenshotOptions;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            return gameObject.HasEventHandlers<IPointerDownHandler>() &&
                   gameObject.HasEventHandlers<IPointerUpHandler>();
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be clicked.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            if (raycastResult.gameObject == null)
            {
                raycastResult = RaycastResultExtensions.CreateFrom(gameObject, GetScreenPoint);
            }

            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            await operationLogger.Log();

            // Do operation
            using (var pointerClickSimulator = new PointerClickEventSimulator(gameObject, raycastResult, Logger))
            {
                await pointerClickSimulator.PointerClickAsync(_holdMillis, cancellationToken: cancellationToken);
            }
        }
    }
}
