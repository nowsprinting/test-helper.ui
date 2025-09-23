// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Random;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using TestHelper.UI.Random;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Swipe operator for Unity UI (uGUI) components.
    /// </summary>
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
            get => _random ?? new RandomWrapper();
            set => _random = value;
        }
        private IRandom _random;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="swipeSpeed">Swipe speed in units per second (must be positive). Default is 2400.</param>
        /// <param name="swipeDistance">Swipe distance (units). Default is 400.</param>
        /// <param name="random">PRNG instance. Default is <see cref="TestHelper.Random.Random.Shared"/>.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="screenshotOptions">Screenshot options.</param>
        public UguiSwipeOperator(
            int swipeSpeed = 2400,
            float swipeDistance = 400f,
            IRandom random = null,
            ILogger logger = null,
            ScreenshotOptions screenshotOptions = null)
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
            Random = random;
            Logger = logger;
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

            // Check if the gameObject has ScrollRect or Scrollbar components
            if (gameObject.TryGetEnabledComponent<ScrollRect>(out _))
            {
                return true;
            }

            if (gameObject.TryGetEnabledComponent<Scrollbar>(out _))
            {
                return true;
            }

            return false;
        }
        
        /// <inheritdoc/>
        public async UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            // Generate random direction based on scrollable directions
            Vector2 direction;
            if (gameObject.TryGetEnabledComponent<ScrollRect>(out var scrollRect))
            {
                var x = GetRandomHorizontalDirection(scrollRect);
                var y = GetRandomVerticalDirection(scrollRect);
                direction = new Vector2(x, y);
            }
            else if (gameObject.TryGetEnabledComponent<Scrollbar>(out var scrollbar))
            {
                var x = GetRandomHorizontalDirection(scrollbar);
                var y = GetRandomVerticalDirection(scrollbar);
                direction = new Vector2(x, y);
            }
            else
            {
                // For other swipeable components, use a random direction
                direction = Random.insideUnitCircle;
            }

            // Ensure direction is not zero (especially for ScrollRect/Scrollbar with only one axis)
            if (direction == Vector2.zero)
            {
                // If both axes are disabled, default to horizontal swipe
                direction = Vector2.right;
            }

            // Call the direction overload
            await OperateAsync(gameObject, direction, raycastResult, cancellationToken);
        }

        private float GetRandomHorizontalDirection(UIBehaviour scroller)
        {
            if (scroller.CanScrollHorizontally())
            {
                return Random.value < 0.5f ? -1f : 1f;
            }

            return 0f;
        }

        private float GetRandomVerticalDirection(UIBehaviour scroller)
        {
            if (scroller.CanScrollVertically())
            {
                return Random.value < 0.5f ? -1f : 1f;
            }

            return 0f;
        }
        
        /// <inheritdoc/>
        public async UniTask OperateAsync(GameObject gameObject, Vector2 direction, RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            if (direction == Vector2.zero)
            {
                throw new ArgumentException("Direction cannot be zero", nameof(direction));
            }

            var startPosition = GetScreenPoint?.Invoke(gameObject) ?? gameObject.transform.position;
            var normalizedDirection = direction.normalized;
            var destination = startPosition + normalizedDirection * _swipeDistance;

            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("direction", normalizedDirection);
            operationLogger.Properties.Add("distance", _swipeDistance);
            operationLogger.Properties.Add("startPosition", startPosition);
            operationLogger.Properties.Add("destination", destination);
            await operationLogger.Log();

            using (var simulator = new PointerDragEventSimulator(gameObject, raycastResult, Logger))
            {
                simulator.BeginDrag();
                await simulator.DragAsync(destination, _swipeSpeed, cancellationToken);
                simulator.EndDrag(out _, out _);
            }
        }
    }
}