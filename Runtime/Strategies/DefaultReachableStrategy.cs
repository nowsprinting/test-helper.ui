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
        /// Default implementation uses <c>DefaultScreenPointStrategy</c>, checks whether a raycast at the pivot position (via <c>EventSystem.RaycastAll</c>) hits the target.
        /// <para>
        /// If that raycast is blocked, it retries inside the visible rect of the <c>RectTransform</c> (clipped by the screen and ancestor <c>RectMask2D</c>/<c>Mask</c>):
        /// each miss subtracts the blocker's screen rect and the next raycast targets the center of the largest remaining strip, up to 5 raycasts in total.
        /// On success the hit point becomes the operating point; on total failure the pivot miss is reported.
        /// </para>
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

            if (Raycast(gameObject, _getScreenPoint.Invoke(gameObject), verboseLogger, out raycastResult))
            {
                return true;
            }

            var firstMiss = raycastResult;
            if (TryReachAroundBlockers(gameObject, firstMiss, verboseLogger, out raycastResult))
            {
                return true;
            }

            // Report the pivot miss so verbose logs and the visualizer describe the point the user configured,
            // not the last fallback point tried.
            raycastResult = firstMiss;
            return false;
        }

        private bool Raycast(GameObject target, Vector2 screenPoint, ILogger verboseLogger, out RaycastResult result)
        {
            var pointerEventData = GetCachedPointerEventData();
            pointerEventData.position = screenPoint;

            _results.Clear();
            EventSystem.current.RaycastAll(pointerEventData, _results);

            _results.RemoveAll(r =>
                !IsSameOrChildObject(target, r.gameObject.transform) &&
                (r.gameObject.TryGetEnabledComponentInParent<NonBlockingAnnotation>(out _) ||
                 IsMatchedOrChildOfNonBlockingMatchersMatched(r.gameObject)));

            if (_results.Count == 0)
            {
                if (verboseLogger != null)
                {
                    var message = new StringBuilder(CreateMessage(target, screenPoint));
                    message.Append(" Raycast is not hit.");
                    verboseLogger.Log(message.ToString());
                }

                result = new RaycastResult() { screenPosition = screenPoint };
                return false;
            }

            var isSameOrChildObject = IsSameOrChildObject(target, _results[0].gameObject.transform);
            if (!isSameOrChildObject && verboseLogger != null)
            {
                var message = new StringBuilder(CreateMessage(target, screenPoint));
                message.Append(" Raycast hit other objects: [");
                foreach (var hit in _results)
                {
                    message.Append($"{hit.gameObject.name}({hit.gameObject.GetId().ToString()})");
                    message.Append(", ");
                }

                message.Remove(message.Length - 2, 2);
                message.Append("]");
                verboseLogger.Log(message.ToString());
            }

            result = _results[0];
            return isSameOrChildObject;
        }

        private bool TryReachAroundBlockers(GameObject target, RaycastResult firstMiss, ILogger verboseLogger,
            out RaycastResult result)
        {
            result = default;
            if (!TryGetVisibleScreenRect(target, out var rect))
            {
                return false;
            }

            var miss = firstMiss;
            for (var raycastCount = 1; raycastCount < MaxRaycastCount; raycastCount++)
            {
                // A miss outside the visible rect (pivot off-screen or under a mask) says nothing about blockers
                // inside it, so the rect is tried as-is instead of subtracting whatever that miss hit.
                if (rect.Contains(miss.screenPosition) && !TrySubtractBlocker(target, miss, ref rect))
                {
                    return false;
                }

                if (Raycast(target, rect.center, verboseLogger, out result))
                {
                    return true;
                }

                miss = result;
            }

            return false;
        }

        /// <summary>
        /// Removes the screen rect of the object hit by <paramref name="miss"/> from <paramref name="rect"/>.
        /// </summary>
        /// <returns>False if the miss cannot be explained by geometry or nothing is left to try</returns>
        private static bool TrySubtractBlocker(GameObject target, RaycastResult miss, ref Rect rect)
        {
            var blocker = miss.gameObject;
            if (blocker == null || // nothing hit
                IsSameOrChildObject(blocker, target.transform) || // an ancestor: alpha hit test etc., not geometry
                !ScreenRectUtility.TryGetScreenRect(blocker, out var blockerRect)) // 3D object
            {
                return false;
            }

            rect = ScreenRectUtility.LargestRemainder(rect, blockerRect);
            return rect.width >= 1f && rect.height >= 1f;
        }

        private static bool TryGetVisibleScreenRect(GameObject target, out Rect rect)
        {
            if (!ScreenRectUtility.TryGetScreenRect(target, out rect))
            {
                return false;
            }

            rect = ScreenRectUtility.Intersect(rect, new Rect(0, 0, Screen.width, Screen.height));

            // ponytail: ignores RectMask2D padding/softness; add if a real UI needs it
            for (var current = target.transform; current != null; current = current.parent)
            {
                var clips = (current.TryGetComponent<RectMask2D>(out var rectMask) && rectMask.isActiveAndEnabled) ||
                            (current.TryGetComponent<Mask>(out var mask) && mask.isActiveAndEnabled);
                if (clips && ScreenRectUtility.TryGetScreenRect(current.gameObject, out var maskRect))
                {
                    rect = ScreenRectUtility.Intersect(rect, maskRect);
                }
            }

            return rect.width >= 1f && rect.height >= 1f;
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
