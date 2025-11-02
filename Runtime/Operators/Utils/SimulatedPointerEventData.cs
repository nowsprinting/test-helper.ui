// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Operators.Utils
{
    /// <summary>
    /// Class that simulates the pointer-down state of <see cref="PointerEventData"/>.
    /// </summary>
    public sealed class SimulatedPointerEventData : PointerEventData, IDisposable
    {
        public enum PointingDeviceType
        {
            Auto,
            Mouse,
            TouchScreen,
        }

        private readonly PointingDeviceType _deviceType; // Mouse or TouchScreen

        private static int s_touchCount; // Touchscreen touches go from 0

        /// <summary>
        /// Constructor for pointer-down state.
        /// </summary>
        /// <param name="gameObject">Event target <c>GameObject</c></param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the game-title implementation.</param>
        /// <param name="deviceType">Can specify mouse or touch screen for tests</param>
        public SimulatedPointerEventData(
            GameObject gameObject,
            RaycastResult raycastResult,
            PointingDeviceType deviceType = PointingDeviceType.Auto)
            : base(EventSystem.current)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (deviceType == PointingDeviceType.Auto)
            {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
                _deviceType = PointingDeviceType.TouchScreen;
#else
                _deviceType = PointingDeviceType.Mouse;
#endif
            }
            else
            {
                _deviceType = deviceType;
            }

            // Set the initial state of the pointer-down event based on RaycastResult
            pointerCurrentRaycast = raycastResult;
            pointerPressRaycast = raycastResult;
            rawPointerPress = raycastResult.gameObject;
#if UNITY_2022_3_OR_NEWER
            displayIndex = raycastResult.displayIndex;
#endif
            position = raycastResult.screenPosition;
            pressPosition = raycastResult.screenPosition;

            // Set the initial state of the pointer-enter event based on target GameObject
            pointerEnter = gameObject;

            if (_deviceType == PointingDeviceType.TouchScreen)
            {
                pointerId = s_touchCount++; // Touchscreen touches go from 0
            }
            else
            {
                pointerId = -1; // Mouse left button
            }

            button = InputButton.Left;
            clickCount = 0; // Note: not yet clicked
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_deviceType == PointingDeviceType.TouchScreen)
            {
                s_touchCount--;
            }
        }

        /// <summary>
        /// Transition state to the pointer-down.
        /// </summary>
        public void SetStateToPointerDowning()
        {
            eligibleForClick = true;
        }

        /// <summary>
        /// Transition state to the after pointer-down.
        /// </summary>
        public void SetStateToPointerDowned()
        {
            pointerPress = pointerEnter;
#if UNITY_2020_3_OR_NEWER
            pointerClick = pointerEnter;
#endif
        }

        /// <summary>
        /// Transition state to the click.
        /// </summary>
        public void SetStateToClicking()
        {
            clickCount++;
        }

        /// <summary>
        /// Transition state to the after click.
        /// </summary>
        public void SetStateToClicked()
        {
            clickTime = Time.unscaledTime;
            eligibleForClick = false;
            pointerPress = null;
#if UNITY_2020_3_OR_NEWER
            pointerClick = null;
#endif
        }

        /// <summary>
        /// Transition state to the begin-drag.
        /// </summary>
        public void SetStateToBeginDrag()
        {
            pointerDrag = pointerEnter;
        }

        /// <summary>
        /// Transition state to the after begin-drag.
        /// </summary>
        public void SetStateToDragging()
        {
            dragging = true;
        }

        /// <summary>
        /// Transition state to the end-drag.
        /// </summary>
        public void SetStateToEndDrag()
        {
            eligibleForClick = false;
            pointerPress = null;
#if UNITY_2020_3_OR_NEWER
            pointerClick = null;
#endif
            pointerDrag = null;
            delta = Vector2.zero;
            // Note: `dragging` is still true here
        }
    }
}
