// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using TestHelper.UI.Annotations;
using TestHelper.UI.Extensions;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Strategies.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.Strategies
{
    /// <summary>
    /// Default strategy to examine whether <c>GameObject</c> is reachable from the user.
    /// </summary>
    public class DefaultReachableStrategy : IReachableStrategy
    {
        // Total raycasts per IsReachable call, including the first one at the pivot.
        private const int MaxRaycastCount = 5;

        private readonly List<RectMask2D> _rectMasks = new List<RectMask2D>();
        private readonly List<Mask> _masks = new List<Mask>();

        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly ILogger _verboseLogger;
        private readonly List<IGameObjectMatcher> _nonBlockingMatchers;

        private readonly List<RaycastResult> _results = new List<RaycastResult>();

        private PointerEventData _cachedPointerEventData;
        private int _cachedFrameCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="getScreenPoint">Function returns the screen point of <c>GameObject</c></param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <param name="nonBlockingMatchers">List of matchers to exclude GameObjects from blocking reachability checks</param>
        public DefaultReachableStrategy(
            Func<GameObject, Vector2> getScreenPoint = null,
            ILogger verboseLogger = null,
            List<IGameObjectMatcher> nonBlockingMatchers = null)
        {
            _getScreenPoint = getScreenPoint ?? DefaultScreenPointStrategy.GetScreenPoint;
            _verboseLogger = verboseLogger;
            _nonBlockingMatchers = nonBlockingMatchers;
        }

        ///<inheritdoc/>
        /// <remarks>
        /// Default implementation uses <c>DefaultScreenPointStrategy</c>, checks whether a raycast from <c>Camera.main</c> to the pivot position passes through.
        /// <para>
        /// GameObjects with <c>NonBlockingAnnotation</c> component (or whose parent has it) are excluded from raycast results,
        /// except for the target GameObject itself and its child objects.
        /// </para>
        /// <para>
        /// GameObjects that match any of the <c>IGameObjectMatcher</c> instances provided in the constructor
        /// (or whose parent matches) are also excluded from raycast results, except for the target GameObject itself and its child objects.
        /// </para>
        /// </remarks>
        public bool IsReachable(GameObject gameObject, out RaycastResult raycastResult, ILogger verboseLogger = null)
        {
            verboseLogger = verboseLogger ?? _verboseLogger; // If null, use the specified in the constructor.

            if (EventSystem.current == null)
            {
                Debug.LogError("EventSystem is not found.");
                raycastResult = default;
                return false;
            }

            var pointerEventData = GetCachedPointerEventData();
            pointerEventData.position = _getScreenPoint.Invoke(gameObject);

            _results.Clear();
            EventSystem.current.RaycastAll(pointerEventData, _results);

            _results.RemoveAll(r =>
                !IsSameOrChildObject(gameObject, r.gameObject.transform) &&
                (r.gameObject.TryGetEnabledComponentInParent<NonBlockingAnnotation>(out _) ||
                 IsMatchedOrChildOfNonBlockingMatchersMatched(r.gameObject)));

            if (_results.Count == 0)
            {
                if (verboseLogger != null)
                {
                    var message = new StringBuilder(CreateMessage(gameObject, pointerEventData.position));
                    message.Append(" Raycast is not hit.");
                    verboseLogger.Log(message.ToString());
                }

                raycastResult = new RaycastResult() { screenPosition = pointerEventData.position };
                return false;
            }

            var isSameOrChildObject = IsSameOrChildObject(gameObject, _results[0].gameObject.transform);
            if (!isSameOrChildObject && verboseLogger != null)
            {
                var message = new StringBuilder(CreateMessage(gameObject, pointerEventData.position));
                message.Append(" Raycast hit other objects: [");
                foreach (var result in _results)
                {
                    message.Append($"{result.gameObject.name}({result.gameObject.GetId().ToString()})");
                    message.Append(", ");
                }

                message.Remove(message.Length - 2, 2);
                message.Append("]");
                verboseLogger.Log(message.ToString());
            }

            raycastResult = _results[0];
            return isSameOrChildObject;
        }

        private bool Raycast(GameObject target, Vector2 screenPoint, ILogger verboseLogger, out RaycastResult result)
        {
            result = default;
            return false;
        }

        private bool TryReachAroundBlockers(GameObject target, RaycastResult firstMiss, ILogger verboseLogger,
            out RaycastResult result)
        {
            result = default;
            return false;
        }

        private bool TryGetVisibleScreenRect(GameObject target, out Rect rect)
        {
            rect = default;
            return false;
        }

        private PointerEventData GetCachedPointerEventData()
        {
            if (_cachedPointerEventData == null || _cachedFrameCount != Time.frameCount)
            {
                _cachedPointerEventData = new PointerEventData(EventSystem.current);
                _cachedFrameCount = Time.frameCount;
            }

            return _cachedPointerEventData;
        }

        private static bool IsSameOrChildObject(GameObject target, Transform hitObjectTransform)
        {
            while (hitObjectTransform)
            {
                if (hitObjectTransform == target.transform)
                {
                    return true;
                }

                hitObjectTransform = hitObjectTransform.transform.parent;
            }

            return false;
        }

        private bool IsMatchedOrChildOfNonBlockingMatchersMatched(GameObject gameObject)
        {
            if (_nonBlockingMatchers == null || _nonBlockingMatchers.Count == 0)
            {
                return false;
            }

            var current = gameObject.transform;
            while (current != null)
            {
                foreach (var matcher in _nonBlockingMatchers)
                {
                    if (matcher.IsMatch(current.gameObject))
                    {
                        return true;
                    }
                }

                current = current.parent;
            }

            return false;
        }

        private static string CreateMessage(GameObject gameObject, Vector2 position)
        {
            var x = (int)position.x;
            var y = (int)position.y;
            var builder = new StringBuilder();
            builder.Append(
                $"Not reachable to {gameObject.name}({gameObject.GetId().ToString()}), position=({x.ToString()},{y.ToString()})");

            var camera = gameObject.GetAssociatedCamera();
            if (camera != null)
            {
                builder.Append($", camera={camera.name}({camera.GetId().ToString()})");
            }

            builder.Append(".");
            return builder.ToString();
        }
    }
}
