// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Operators.Utils
{
    /// <summary>
    /// Simulator class to reproduce pointer events.
    /// </summary>
    public sealed class PointerEventSimulator : IDisposable
    {
        private readonly GameObject _gameObject;
        private readonly ILogger _logger;
        private readonly bool _hasSelectable;
        private readonly SimulatedPointerEventData _eventData;
        private readonly List<RaycastResult> _raycastResults;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObject">Click target <c>GameObject</c></param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the game-title implementation.</param>
        /// <param name="logger">Logger set if you need</param>
        public PointerEventSimulator(GameObject gameObject, RaycastResult raycastResult, ILogger logger = null)
        {
            _gameObject = gameObject;
            _logger = logger;
            _hasSelectable = gameObject.TryGetEnabledComponent<Selectable>(out _);
            _eventData = new SimulatedPointerEventData(gameObject, raycastResult);
            _raycastResults = new List<RaycastResult>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _eventData.Dispose();
        }

        /// <summary>
        /// Reproduce the sequence of events that happen when pointer-clicking.
        /// <list type="number">
        ///     <item><c>OnPointerEnter</c> (once at the beginning)</item>
        ///     <item><c>OnSelect</c> (once at the beginning, if <c>Selectable</c> is attached)</item>
        ///     <item>For each click:</item>
        ///     <item>-- <c>OnPointerDown</c></item>
        ///     <item>-- <c>OnInitializePotentialDrag</c></item>
        ///     <item>-- Wait for the hold time</item>
        ///     <item>-- <c>OnPointerUp</c></item>
        ///     <item>-- <c>OnPointerClick</c></item>
        ///     <item>-- Wait for interval</item>
        ///     <item><c>OnPointerExit</c> (once at the end)</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// <c>OnDeselect</c> event is called by the system when the focus moves to another element, so it is not called in this method.
        /// </remarks>
        /// <param name="holdMillis">Hold time in milliseconds if click-and-hold</param>
        /// <param name="clickCount">Number of clicks</param>
        /// <param name="intervalMillis">Interval between clicks in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async UniTask PointerClickAsync(int holdMillis = 0, int clickCount = 1, int intervalMillis = 0,
            CancellationToken cancellationToken = default)
        {
            var gameObjectNameCache = _gameObject.name;

            // Enter (once at the beginning)
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerEnterHandler);
            if (_hasSelectable)
            {
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.selectHandler);
            }

            // Multiple clicks
            for (var i = 0; i < clickCount; i++)
            {
                // Down
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerDownHandler);
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.initializePotentialDrag);

                if (holdMillis > 0)
                {
                    await UniTask.Delay(holdMillis, ignoreTimeScale: true, cancellationToken: cancellationToken);
                }
                else
                {
                    await UniTask.NextFrame(cancellationToken: cancellationToken);
                }

                // Up
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerUpHandler);
                _eventData.SetStateToClicking();
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerClickHandler);
                _eventData.SetStateToClicked();

                // Wait for interval for multiple clicks
                if (intervalMillis <= 0)
                {
                    continue;
                }

                await UniTask.Delay(intervalMillis, ignoreTimeScale: true, cancellationToken: cancellationToken);
                if (_gameObject != null)
                {
                    continue;
                }

                _logger?.Log($"{gameObjectNameCache} is destroyed before pointer-up event.");
                return;
            }

            // Exit (once at the end)
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerExitHandler);
        }

        /// <summary>
        /// Reproduce the sequence of events that happen when drag and drop.
        /// <list type="number">
        ///     <item><c>OnPointerEnter</c></item>
        ///     <item><c>OnSelect</c> (if <c>Selectable</c> is attached)</item>
        ///     <item><c>OnPointerDown</c></item>
        ///     <item><c>OnInitializePotentialDrag</c></item>
        ///     <item><c>OnBeginDrag</c></item>
        ///     <item>Every frame until the drag is complete:</item>
        ///     <item>-- <c>OnPointerMove</c> to the current pointed position using raycaster</item>
        ///     <item>-- <c>OnDrag</c></item>
        ///     <item>Wait for <c>delayBeforeDrop</c></item>
        ///     <item><c>OnPointerUp</c></item>
        ///     <item><c>OnDrop</c> to the current pointed position using raycaster</item>
        ///     <item><c>OnEndDrag</c></item>
        ///     <item><c>OnPointerExit</c></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// <c>OnDeselect</c> event is called by the system when the focus moves to another element, so it is not called in this method.
        /// </remarks>
        /// <param name="destination">Drop destination point. Drag speed is assumed to be specified in the constructor.</param>
        /// <param name="dragSpeed">Drag amount per frame (must be positive)</param>
        /// <param name="delayBeforeDrop">Delay in seconds after dragging is complete and before dropping. You can also use it to keep an On-Screen Stick in place.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async UniTask DragAndDropAsync(Vector2 destination, float dragSpeed, double delayBeforeDrop,
            CancellationToken cancellationToken = default)
        {
            var gameObjectNameCache = _gameObject.name;

            // Enter
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerEnterHandler);
            if (_hasSelectable)
            {
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.selectHandler);
            }

            // Begin drag
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.initializePotentialDrag);
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.beginDragHandler);

            // Dragging
            while (!cancellationToken.IsCancellationRequested)
            {
                var currentPosition = _eventData.position;
                var direction = (destination - currentPosition).normalized;
                var distance = Vector2.Distance(currentPosition, destination);
                if (distance < dragSpeed)
                {
                    _eventData.position = destination;
                    break;
                }

                _eventData.position = currentPosition + direction * dragSpeed;

                if (TryGetGameObjectAtCurrentPosition(out var pointerGameObject))
                {
                    ExecuteEvents.ExecuteHierarchy(pointerGameObject, _eventData, ExecuteEvents.pointerMoveHandler);
                }

                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.dragHandler);

                await UniTask.NextFrame(cancellationToken: cancellationToken);
                if (_gameObject != null)
                {
                    continue;
                }

                _logger?.Log($"{gameObjectNameCache} is destroyed before pointer-up event.");
                return;
            }

            // Wait for delay before drop
            await UniTask.Delay(TimeSpan.FromSeconds(delayBeforeDrop), ignoreTimeScale: true,
                cancellationToken: cancellationToken);

            // Pointer up
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerUpHandler);

            // Drop
            if (TryGetGameObjectAtCurrentPosition(out var dropGameObject))
            {
                ExecuteEvents.ExecuteHierarchy(dropGameObject, _eventData, ExecuteEvents.dropHandler);
            }

            // End drag
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.endDragHandler);
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerExitHandler);
        }

        private bool TryGetGameObjectAtCurrentPosition(out GameObject gameObject)
        {
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(_eventData, _raycastResults);

            if (_raycastResults.Count > 0)
            {
                gameObject = _raycastResults[0].gameObject;
                return true;
            }

            gameObject = null;
            return false;
        }
    }
}
