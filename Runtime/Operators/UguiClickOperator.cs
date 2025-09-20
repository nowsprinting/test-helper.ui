// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Click (tap) operator for Unity UI (uGUI) components.
    /// </summary>
    public class UguiClickOperator : IClickOperator
    {
        /// <inheritdoc/>
        public ILogger Logger { protected get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { protected get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console)</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        public UguiClickOperator(ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
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
            using (var pointerClickSimulator = new PointerClickEventSimulator(gameObject, raycastResult))
            {
                await pointerClickSimulator.PointerClickAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
