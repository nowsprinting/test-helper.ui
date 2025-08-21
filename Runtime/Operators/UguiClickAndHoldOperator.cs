// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Click and hold operator for Unity UI (uGUI) components.
    /// a.k.a. touch and hold, long press.
    /// </summary>
    public class UguiClickAndHoldOperator : IClickAndHoldOperator
    {
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        private readonly int _holdMillis;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="holdMillis">Hold time in milliseconds</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiClickAndHoldOperator(int holdMillis = 1000,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            _holdMillis = holdMillis;
            Logger = logger ?? Debug.unityLogger;
            ScreenshotOptions = screenshotOptions;
        }

        /// <inheritdoc />
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject.TryGetEnabledComponent<EventTrigger>(out var eventTrigger))
            {
                return eventTrigger.triggers.Any(x => x.eventID == EventTriggerType.PointerDown) &&
                       eventTrigger.triggers.Any(x => x.eventID == EventTriggerType.PointerUp);
            }

            return gameObject.TryGetEnabledComponent<IPointerDownHandler>(out _) &&
                   gameObject.TryGetEnabledComponent<IPointerUpHandler>(out _);
        }

        /// <inheritdoc />
        /// <remarks>
        /// This method receives <c>RaycastResult</c>, but passing <c>default</c> may be OK, depending on the component being operated on.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
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
