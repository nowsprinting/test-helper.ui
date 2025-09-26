// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Random;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using TestHelper.UI.Random;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Scroll wheel operator for Unity UI (uGUI) components that implements IScrollHandler.
    /// </summary>
    /// <remarks>
    /// If no scroll destination is specified (e.g., in monkey testing), a random scroll destinations will be used.
    /// </remarks>
    public class UguiScrollWheelOperator : IScrollWheelOperator, IRandomizable, IScreenPointCustomizable
    {
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        /// <inheritdoc/>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; }

        /// <inheritdoc/>
        public IRandom Random
        {
            private get
            {
                if (_random == null)
                {
                    _random = new RandomWrapper();
                }

                return _random;
            }
            set
            {
                _random = value;
            }
        }

        private IRandom _random;

        private readonly int _scrollSpeed;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="scrollSpeed">Scroll speed in units per second (must be positive)</param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <param name="random">PRNG instance</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need</param>
        /// <exception cref="ArgumentException">Thrown when scrollPerFrame is zero or negative</exception>
        public UguiScrollWheelOperator(int scrollSpeed = 1200, Func<GameObject, Vector2> getScreenPoint = null,
            IRandom random = null, ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            if (scrollSpeed <= 0)
            {
                throw new ArgumentException("scrollSpeed must be positive", nameof(scrollSpeed));
            }

            _scrollSpeed = scrollSpeed;
            GetScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _random = random;
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

            return gameObject.HasEventHandlers<IScrollHandler>();
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start scrolling.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            // Generate a random direction with respect to scrollable directions
            var direction = GenerateRandomScrollDirection(gameObject);

            // Generate random distance (10 to max distance)
            var maxDistance = CalcMaxScrollDistance(gameObject);
            var distance = Random.Next(10, maxDistance);

            // Call the direction/distance overload
            await OperateAsync(gameObject, direction, distance, raycastResult, cancellationToken);
        }

        private Vector2 GenerateRandomScrollDirection(GameObject gameObject)
        {
            // Check for scrollable components
            if (gameObject.TryGetEnabledComponent<ScrollRect>(out var scrollRect))
            {
                return GenerateDirectionForScrollable(scrollRect);
            }

            if (gameObject.TryGetEnabledComponent<Scrollbar>(out var scrollbar))
            {
                return GenerateDirectionForScrollable(scrollbar);
            }

            // Default to random direction for non-scrollable components
            return Random.insideUnitCircle;
        }

        private Vector2 GenerateDirectionForScrollable(UIBehaviour scrollable)
        {
            var x = GetRandomHorizontalDirection(scrollable);
            var y = GetRandomVerticalDirection(scrollable);
            return new Vector2(x, y);
        }

        private float GetRandomHorizontalDirection(UIBehaviour scroller)
        {
            if (scroller.CanScrollHorizontally())
            {
                return Random.value < 0.5 ? -1f : 1f;
            }

            return 0f;
        }

        private float GetRandomVerticalDirection(UIBehaviour scroller)
        {
            if (scroller.CanScrollVertically())
            {
                return Random.value < 0.5 ? -1f : 1f;
            }

            return 0f;
        }

        private static int CalcMaxScrollDistance(GameObject gameObject)
        {
            if (gameObject.TryGetEnabledComponent<RectTransform>(out var rectTransform))
            {
                return (int)Math.Max(rectTransform.rect.width, rectTransform.rect.height);
            }

            return Math.Max(Screen.width, Screen.height);
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start scrolling.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, Vector2 direction, int distance,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            // Validate parameters
            if (direction.magnitude == 0f)
            {
                throw new ArgumentException("Direction must not be zero", nameof(direction));
            }

            if (distance <= 0)
            {
                throw new ArgumentException("Distance must be positive", nameof(distance));
            }

            // Log direction and distance
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("direction", direction);
            operationLogger.Properties.Add("distance", distance);
            await operationLogger.Log();

            // Calculate destination from direction and distance
            var flipped = direction * new Vector2(-1, 1); // flip X axis to match scroll wheel direction
            var destination = flipped.normalized * distance;

            // Call the common implementation
            await OperateAsyncCore(gameObject, destination, raycastResult, cancellationToken);
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start scrolling.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        [Obsolete("Use OperateAsync with direction and distance parameters instead.")]
        public async UniTask OperateAsync(GameObject gameObject, Vector2 destination,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            // Log destination
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("destination", destination);
            await operationLogger.Log();

            // Call the common implementation
            await OperateAsyncCore(gameObject, destination, raycastResult, cancellationToken);
        }

        private async UniTask OperateAsyncCore(GameObject gameObject, Vector2 destination,
            RaycastResult raycastResult, CancellationToken cancellationToken)
        {
            if (raycastResult.gameObject == null)
            {
                raycastResult = RaycastResultExtensions.CreateFrom(gameObject, GetScreenPoint);
            }

            // Send pointer enter event
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = raycastResult.screenPosition
            };
            ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);

            // Perform scroll operation
            var remainingDistance = destination.magnitude;
            var direction = destination.normalized;

            while (remainingDistance > 0 && !cancellationToken.IsCancellationRequested)
            {
                var frameSpeed = _scrollSpeed * Time.deltaTime;
                var scrollDelta = direction * Mathf.Min(frameSpeed, remainingDistance);
                pointerEventData.scrollDelta = scrollDelta;

                ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.scrollHandler);

                remainingDistance -= frameSpeed;

                await UniTask.Yield(cancellationToken);
            }

            // Send pointer exit event
            ExecuteEvents.ExecuteHierarchy(gameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
        }
    }
}
