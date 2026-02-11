// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestHelper.UI.Annotations;
using TestHelper.UI.Extensions;
using TestHelper.UI.GameObjectMatchers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI.Strategies
{
    /// <summary>
    /// Default strategy to examine whether <c>GameObject</c> is reachable from the user.
    /// </summary>
    public class DefaultReachableStrategy : IReachableStrategy
    {
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
                    message.Append($"{result.gameObject.name}({result.gameObject.GetInstanceID()})");
                    message.Append(", ");
                }

                message.Remove(message.Length - 2, 2);
                message.Append("]");
                verboseLogger.Log(message.ToString());
            }

            raycastResult = _results[0];
            return isSameOrChildObject;
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
                if (_nonBlockingMatchers.Any(matcher => matcher.IsMatch(current.gameObject)))
                {
                    return true;
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
            builder.Append($"Not reachable to {gameObject.name}({gameObject.GetInstanceID()}), position=({x},{y})");

            var camera = gameObject.GetAssociatedCamera();
            if (camera != null)
            {
                builder.Append($", camera={camera.name}({camera.GetInstanceID()})");
            }

            builder.Append(".");
            return builder.ToString();
        }
    }
}
