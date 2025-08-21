// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Random;
using TestHelper.UI.Annotations;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Drag and drop operator for Unity UI (uGUI) components.
    /// </summary>
    public class UguiDragAndDropOperator : IDragAndDropOperator
    {
        /// <inheritdoc/>
        public ILogger Logger { get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { get; set; }

        private readonly IRandom _random;
        private readonly IReachableStrategy _reachableStrategy;
        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly float _dragSpeed;
        private readonly double _delayBeforeDrop;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dragSpeed">Drag amount per frame (must be positive).</param>
        /// <param name="delayBeforeDrop">Delay in seconds after dragging is complete and before dropping. You can also use it to keep an On-screen stick in place.</param>
        /// <param name="random">PRNG instance.</param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c>. Used to determine drop position.</param>
        /// <param name="reachableStrategy">Strategy to examine whether <c>GameObject</c> is reachable from the user.</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console).</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need.</param>
        public UguiDragAndDropOperator(float dragSpeed = 10.0f, double delayBeforeDrop = 0.0d, IRandom random = null,
            Func<GameObject, Vector2> getScreenPoint = null, IReachableStrategy reachableStrategy = null,
            ILogger logger = null, ScreenshotOptions screenshotOptions = null)
        {
            if (dragSpeed <= 0)
            {
                throw new ArgumentException("dragSpeed must be positive", nameof(dragSpeed));
            }

            if (delayBeforeDrop < 0)
            {
                throw new ArgumentException("delayBeforeDrop must be positive or zero", nameof(delayBeforeDrop));
            }

            _dragSpeed = dragSpeed;
            _delayBeforeDrop = delayBeforeDrop;
            _random = random ?? new RandomWrapper();
            _getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _reachableStrategy = reachableStrategy ?? new DefaultReachableStrategy(_getScreenPoint);
            Logger = logger ?? Debug.unityLogger;
            ScreenshotOptions = screenshotOptions;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <see cref="IDropHandler"/> is not required.
        /// </remarks>
        public bool CanOperate(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return false;
            }

            if (gameObject.TryGetEnabledComponent<EventTrigger>(out var eventTrigger))
            {
                if (eventTrigger.CanHandle<IDragHandler>())
                {
                    return true;
                }
            }

            return gameObject.TryGetEnabledComponent<IDragHandler>(out _);
        }

        /// <inheritdoc />
        /// <remarks>
        /// The drop positions are determined in the following order:
        /// <list type="number">
        ///     <item>Drop to the position that <c>GameObject</c> with <see cref="DropAnnotation"/> component if it exists. It will be random if there are multiple</item>
        ///     <item>Drop to the position that <c>GameObject</c> with implement <see cref="IDropHandler"/> component if it exists. It will be random if there are multiple</item>
        ///     <item>Drop to the random screen position</item>
        /// </list>
        /// </remarks>
        public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
            CancellationToken cancellationToken = default)
        {
            var dropAnnotations = FindDropAnnotations() as Component[];
            var dropAnnotation = LotteryComponent(dropAnnotations);
            if (dropAnnotation != null)
            {
                return OperateAsync(gameObject, dropAnnotation.gameObject, raycastResult, cancellationToken);
            }

            var dropHandlers = InteractableComponentsFinder.FindEventHandlers<IDropHandler>().ToArray<Component>();
            var dropHandler = LotteryComponent(dropHandlers);
            if (dropHandler != null)
            {
                return OperateAsync(gameObject, dropHandler.gameObject, raycastResult, cancellationToken);
            }

            var destination = _random.NextScreenPosition();
            return OperateAsync(gameObject, destination, raycastResult, cancellationToken);
        }

        private static IEnumerable<DropAnnotation> FindDropAnnotations()
        {
#if UNITY_2022_3_OR_NEWER
            return Object.FindObjectsByType<DropAnnotation>(FindObjectsSortMode.None);
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return Object.FindObjectsOfType<DropAnnotation>();
#endif
        }

        internal Component LotteryComponent(Component[] components)
        {
            if (components == null)
            {
                return null;
            }

            var list = new List<Component>(components);
            while (list.Count > 0)
            {
                var index = _random.Next(list.Count);
                var current = list[index];

                if (_reachableStrategy.IsReachable(current.gameObject, out _))
                {
                    return current;
                }

                list.RemoveAt(index);
            }

            return null;
        }

        /// <inheritdoc />
        public UniTask OperateAsync(GameObject gameObject, GameObject destination,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            var destinationPoint = _getScreenPoint.Invoke(destination);
            return OperateAsync(gameObject, destinationPoint, raycastResult, cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask OperateAsync(GameObject gameObject, Vector2 destination,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            await operationLogger.Log();

            // Do operation
            using (var simulator = new PointerDragEventSimulator(gameObject, raycastResult, Logger))
            {
                simulator.BeginDrag();
                await simulator.DragAsync(destination, _dragSpeed, cancellationToken);

                // Wait for delay before drop
                await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeDrop), ignoreTimeScale: true,
                    cancellationToken: cancellationToken);

                simulator.EndDrag();
            }
        }
    }
}
