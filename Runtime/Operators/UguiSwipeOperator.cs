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
    /// Swipe operator (including flick) for Unity UI (uGUI) components.
    /// </summary>
    /// <remarks>
    /// If a flick operation is required, increasing the swipe speed parameter will determine it as a flick.
    /// </remarks>
    public class UguiSwipeOperator : ISwipeOperator, IRandomizable, IScreenPointCustomizable
    {
        private readonly int _swipeSpeed;
        private readonly float _swipeDistance;

        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        /// <inheritdoc/>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; }

        /// <inheritdoc/>
        public IRandom Random
        {
            private get => _random ?? new RandomWrapper();
            set => _random = value;
        }

        private IRandom _random;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="swipeSpeed">Swipe speed in units per second (must be positive). If a flick operation is required, increase this parameter.</param>
        /// <param name="swipeDistance">Swipe distance (must be positive).</param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c>.</param>
        /// <param name="random">PRNG instance.</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console).</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need.</param>
        public UguiSwipeOperator(int swipeSpeed = 1200, float swipeDistance = 200f,
            Func<GameObject, Vector2> getScreenPoint = null, IRandom random = null,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            if (swipeSpeed <= 0)
            {
                throw new ArgumentException("Swipe speed must be positive", nameof(swipeSpeed));
            }

            if (swipeDistance <= 0)
            {
                throw new ArgumentException("Swipe distance must be positive", nameof(swipeDistance));
            }

            _swipeSpeed = swipeSpeed;
            _swipeDistance = swipeDistance;
            GetScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            Random = random;
            Logger = logger ?? Debug.unityLogger;
            ScreenshotOptions = screenshotOptions;
        }

        /// <inheritdoc/>
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            // Check if the gameObject has IDragHandler (including EventTrigger support)
            if (gameObject.HasEventHandlers<IDragHandler>())
            {
                return true;
            }

            // Check if the gameObject has both IPointerDownHandler and IPointerUpHandler
            if (gameObject.HasEventHandlers<IPointerDownHandler>() &&
                gameObject.HasEventHandlers<IPointerUpHandler>())
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start dragging.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            // Generate a random direction with respect to scrollable directions
            var direction = GenerateRandomSwipeDirection(gameObject);

            // Call the direction overload
            await OperateAsync(gameObject, direction, raycastResult, cancellationToken);
        }

        private Vector2 GenerateRandomSwipeDirection(GameObject gameObject)
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

        /// <inheritdoc/>
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start scrolling.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, Vector2 direction,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            if (direction == Vector2.zero)
            {
                throw new ArgumentException("Direction must not be zero", nameof(direction));
            }

            if (raycastResult.gameObject == null)
            {
                raycastResult = RaycastResultExtensions.CreateFrom(gameObject, GetScreenPoint);
            }

            var canvas = gameObject.GetComponentInParent<Canvas>();
            var scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
            var scaledDistance = _swipeDistance * scaleFactor;
            var scaledSpeed = (int)(_swipeSpeed * scaleFactor);

            // Log direction and distance
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            operationLogger.Properties.Add("direction", direction);
            await operationLogger.Log();

            var normalizedDirection = direction.normalized;
            var destination = raycastResult.screenPosition + normalizedDirection * scaledDistance;

            using (var simulator = new PointerDragEventSimulator(gameObject, raycastResult, Logger))
            {
                simulator.BeginDrag();
                await simulator.DragAsync(destination, scaledSpeed, cancellationToken);
                simulator.EndDrag(out _, out _);
            }
        }
    }
}
