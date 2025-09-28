// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Random;
using TestHelper.UI.Annotations;
using TestHelper.UI.Extensions;
using TestHelper.UI.Operators.Utils;
using TestHelper.UI.Random;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Drag and drop operator for Unity UI (uGUI) components.
    /// </summary>
    /// <remarks>
    /// If no drop destination is specified (e.g., in monkey testing), drop destinations are determined in the following order:
    /// <list type="number">
    ///     <item>Drop to the position that <c>GameObject</c> with <see cref="DropAnnotation"/> component if it exists. It will be random if there are multiple.</item>
    ///     <item>Drop to the position that <c>GameObject</c> with implement <see cref="IDropHandler"/> component if it exists. It will be random if there are multiple.</item>
    ///     <item>Drop to the random screen position.</item>
    /// </list>
    /// </remarks>
    public class UguiDragAndDropOperator : IDragAndDropOperator, IRandomizable, IScreenPointCustomizable,
        IReachableStrategyCustomizable
    {
        /// <inheritdoc/>
        public ILogger Logger { private get; set; }

        /// <inheritdoc/>
        public ScreenshotOptions ScreenshotOptions { private get; set; }

        /// <inheritdoc/>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; }

        /// <inheritdoc/>
        public IReachableStrategy ReachableStrategy { private get; set; }

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

        private readonly int _dragSpeed;
        private readonly double _delayBeforeDrop;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dragSpeed">Drag speed in units per second (must be positive).</param>
        /// <param name="delayBeforeDrop">Delay in seconds after dragging is complete and before dropping. You can also use it to keep an On-screen stick in place.</param>
        /// <param name="random">PRNG instance.</param>
        /// <param name="getScreenPoint">Function returns the screen position of <c>GameObject</c></param>
        /// <param name="reachableStrategy">Strategy to examine whether <c>GameObject</c> is reachable from the user. Used to determine drop position.</param>
        /// <param name="logger">Logger, if omitted, use Debug.unityLogger (output to console).</param>
        /// <param name="screenshotOptions">Take screenshot options set if you need.</param>
        public UguiDragAndDropOperator(int dragSpeed = 1200, double delayBeforeDrop = 0D, IRandom random = null,
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
            _random = random;
            GetScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            ReachableStrategy = reachableStrategy ?? new DefaultReachableStrategy(GetScreenPoint);
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

            return gameObject.HasEventHandlers<IDragHandler>();
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start dragging.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// <br/>
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
            var dropAnnotations = FindDropAnnotations().Where(x => x.isActiveAndEnabled).ToArray<Component>();
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

            var destination = Random.NextScreenPosition();
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
                var index = Random.Next(list.Count);
                var current = list[index];

                if (ReachableStrategy.IsReachable(current.gameObject, out _))
                {
                    return current;
                }

                list.RemoveAt(index);
            }

            return null;
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start dragging.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public UniTask OperateAsync(GameObject gameObject, GameObject destination,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            var destinationPoint = GetScreenPoint.Invoke(destination);
            return OperateAsync(gameObject, destinationPoint, raycastResult, cancellationToken);
        }

        /// <inheritdoc />
        /// <remarks>
        /// If <c>raycastResult</c> is omitted, the pivot position of the <c>gameObject</c> will be used to start dragging.
        /// Screen position is calculated using the <c>getScreenPoint</c> function specified in the constructor.
        /// </remarks>
        public async UniTask OperateAsync(GameObject gameObject, Vector2 destination,
            RaycastResult raycastResult = default, CancellationToken cancellationToken = default)
        {
            if (raycastResult.gameObject == null)
            {
                raycastResult = RaycastResultExtensions.CreateFrom(gameObject, GetScreenPoint);
            }

            var canvas = gameObject.GetComponentInParent<Canvas>();
            var scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
            var scaledSpeed = (int)(_dragSpeed * scaleFactor);

            // Output log before the operation, after the shown effects
            var operationLogger = new OperationLogger(gameObject, this, Logger, ScreenshotOptions);
            operationLogger.Properties.Add("position", raycastResult.screenPosition);
            operationLogger.Properties.Add("destination", destination);
            await operationLogger.Log();

            // Do operation
            using (var simulator = new PointerDragEventSimulator(gameObject, raycastResult, Logger))
            {
                simulator.BeginDrag();
                await simulator.DragAsync(destination, scaledSpeed, cancellationToken);

                // Wait for delay before drop
                await UniTask.Delay(TimeSpan.FromSeconds(_delayBeforeDrop), ignoreTimeScale: true,
                    cancellationToken: cancellationToken);

                simulator.EndDrag(out var dropGameObject, out var dropPosition);
                if (dropGameObject != null)
                {
                    var builder = new StringBuilder();
                    builder.Append($"{this.GetType().Name} drop to {dropGameObject.name}");
                    builder.Append($"({dropGameObject.GetInstanceID()})");
                    builder.Append($", position={Format(dropPosition)}");
                    Logger.Log(builder.ToString());
                }
            }
        }

        private static string Format(Vector2 vector2)
        {
            return $"({vector2.x:F0},{vector2.y:F0})"; // format as an integer because the screen position
        }
    }
}
