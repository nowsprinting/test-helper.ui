// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey.Extensions;
using TestHelper.Monkey.Operators.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TestHelper.Monkey.Operators
{
    /// <summary>
    /// Double click (tap) operator for Unity UI (uGUI) components.
    /// </summary>
    public class UguiDoubleClickOperator : IDoubleClickOperator
    {
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        private readonly int _intervalMillis;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="intervalMillis">Double click interval in milliseconds. Must be positive.</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiDoubleClickOperator(int intervalMillis = 100,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            if (intervalMillis <= 0)
            {
                throw new ArgumentException("Interval must be positive", nameof(intervalMillis));
            }

            _intervalMillis = intervalMillis;
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

            if (gameObject.TryGetEnabledComponent<EventTrigger>(out var eventTrigger))
            {
                return eventTrigger.triggers.Any(x => x.eventID == EventTriggerType.PointerClick);
            }

            return gameObject.TryGetEnabledComponent<IPointerClickHandler>(out _);
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method receives <c>RaycastResult</c>, but passing <c>default</c> may be OK, depending on the component being operated on.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            if (gameObject == null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }

            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            await operationLogger.Log();

            // Do double click using the new multiple click method
            using (var pointerClickSimulator = new PointerEventSimulator(gameObject, raycastResult, Logger))
            {
                await pointerClickSimulator.PointerClickAsync(0, 2, _intervalMillis, cancellationToken);
            }
        }

        [Obsolete(
            "Use OperateAsync(GameObject, RaycastResult, ILogger, ScreenshotOptions, CancellationToken) and properties instead.")]
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null,
            CancellationToken cancellationToken = default)
        {
            if (logger != null)
            {
                Logger = logger;
            }

            if (screenshotOptions != null)
            {
                ScreenshotOptions = screenshotOptions;
            }

            await OperateAsync(gameObject, raycastResult, cancellationToken);
        }
    }
}
