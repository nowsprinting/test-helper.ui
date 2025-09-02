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
    /// A class that simulates drag and drop events.
    /// </summary>
    public sealed class PointerDragEventSimulator : IDisposable
    {
        private readonly GameObject _gameObject;
        private readonly string _gameObjectNameCache;
        private readonly bool _hasSelectable;
        private readonly SimulatedPointerEventData _eventData;
        private readonly ILogger _logger;

        private bool _isDragging;
        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObject">Drag target <c>GameObject</c></param>
        /// <param name="raycastResult"><c>RaycastResult</c> includes the screen position of the starting operation. Passing <c>default</c> may be OK, depending on the game-title implementation.</param>
        /// <param name="logger">Verbose logger set if you need</param>
        public PointerDragEventSimulator(GameObject gameObject, RaycastResult raycastResult, ILogger logger = null)
        {
            _gameObject = gameObject;
            _gameObjectNameCache = gameObject.name;
            _hasSelectable = gameObject.TryGetEnabledComponent<Selectable>(out _);
            _eventData = new SimulatedPointerEventData(gameObject, raycastResult);
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_isDragging)
            {
                _logger?.Log(LogType.Warning, $"{_gameObjectNameCache}.Dispose method was called while dragging.");
                EndDrag(out _, out _);
            }

            _eventData.Dispose();
        }

        /// <summary>
        /// Reproduce the sequence of events that happen when beginning drag.
        /// <list type="number">
        ///     <item><c>OnPointerEnter</c></item>
        ///     <item><c>OnSelect</c> (if <c>Selectable</c> is attached)</item>
        ///     <item><c>OnPointerDown</c></item>
        ///     <item><c>OnInitializePotentialDrag</c></item>
        ///     <item><c>OnBeginDrag</c></item>
        /// </list>
        /// </summary>
        public void BeginDrag()
        {
            if (_isDragging)
            {
                throw new InvalidOperationException("BeginDrag method was called while dragging.");
            }

            if (_gameObject == null)
            {
                _logger?.Log($"{_gameObjectNameCache} is destroyed before beginning drag.");
                return;
            }

            // Pointer enter
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerEnterHandler);
            if (_hasSelectable)
            {
                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.selectHandler);
            }

            // Begin drag
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.initializePotentialDrag);
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.beginDragHandler);

            _isDragging = true;
        }

        /// <summary>
        /// Reproduce the sequence of events that happen when dragging.
        /// <br/>
        ///  Every frame until the drag is complete:
        /// <list type="number">
        ///     <item><c>OnPointerMove</c> to the current pointed position using raycaster</item>
        ///     <item><c>OnDrag</c></item>
        /// </list>
        /// </summary>
        /// <param name="destination">Drop destination point.</param>
        /// <param name="speed">Drag amount per frame (must be positive)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async UniTask DragAsync(Vector2 destination, float speed, CancellationToken cancellationToken = default)
        {
            if (!_isDragging)
            {
                throw new InvalidOperationException("DragAsync method was called before the beginning of the drag.");
            }

            // Dragging
            while (!cancellationToken.IsCancellationRequested)
            {
                var currentPosition = _eventData.position;
                var direction = (destination - currentPosition).normalized;
                var distance = Vector2.Distance(currentPosition, destination);
                if (distance < speed)
                {
                    _eventData.position = destination;
                    break;
                }

                _eventData.position = currentPosition + direction * speed;

#if ENABLE_UGUI2
                if (TryGetGameObjectAtCurrentPosition(out var pointerGameObject))
                {
                    ExecuteEvents.ExecuteHierarchy(pointerGameObject, _eventData, ExecuteEvents.pointerMoveHandler);
                }
#endif

                if (_gameObject == null)
                {
                    _logger?.Log($"{_gameObjectNameCache} is destroyed before dragging.");
                    return;
                }

                ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.dragHandler);

                await UniTask.NextFrame(cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Reproduce the sequence of events that happen when ending drag.
        /// <list type="number">
        ///     <item><c>OnPointerUp</c></item>
        ///     <item><c>OnDrop</c> to the current pointed position using raycaster</item>
        ///     <item><c>OnEndDrag</c></item>
        ///     <item><c>OnPointerExit</c></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// <c>OnDeselect</c> event is called by the system when the focus moves to another element, so it is not called in this method.
        /// </remarks>
        public void EndDrag(out GameObject dropGameObject, out Vector2 dropPosition)
        {
            if (!_isDragging)
            {
                throw new InvalidOperationException("EndDrag method was called before the beginning of the drag.");
            }

            if (_gameObject == null)
            {
                _logger?.Log($"{_gameObjectNameCache} is destroyed before ending drag.");
                dropGameObject = null;
                dropPosition = default;
                return;
            }

            // Pointer up
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerUpHandler);

            // Drop
            if (TryGetGameObjectAtCurrentPosition(out var gameObject))
            {
                dropGameObject = gameObject;
                dropPosition = _eventData.position;
                ExecuteEvents.ExecuteHierarchy(dropGameObject, _eventData, ExecuteEvents.dropHandler);
            }
            else
            {
                dropGameObject = null;
                dropPosition = default;
            }

            // End drag
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.endDragHandler);
            ExecuteEvents.ExecuteHierarchy(_gameObject, _eventData, ExecuteEvents.pointerExitHandler);

            _isDragging = false;
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
