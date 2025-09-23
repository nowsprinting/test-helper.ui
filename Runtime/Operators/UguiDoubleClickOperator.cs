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
    /// Double click (tap) operator for Unity UI (uGUI) components.
    /// </summary>
    public class UguiDoubleClickOperator : IDoubleClickOperator, IScreenPointCustomizable
    {
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        /// <inheritdoc/>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; }

        private readonly int _intervalMillis;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="intervalMillis">Double click interval in milliseconds. Must be positive.</param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiDoubleClickOperator(int intervalMillis = 100, Func<GameObject, Vector2> getScreenPoint = null,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            if (intervalMillis <= 0)
            {
                throw new ArgumentException("Interval must be positive", nameof(intervalMillis));
            }

            _intervalMillis = intervalMillis;
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

            return gameObject.HasEventHandlers<IPointerClickHandler>();
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

            // Do double click using the new multiple click method
            using (var pointerClickSimulator = new PointerClickEventSimulator(gameObject, raycastResult))
            {
                await pointerClickSimulator.PointerClickAsync(0, 2, _intervalMillis, cancellationToken);
            }
        }
    }
}
