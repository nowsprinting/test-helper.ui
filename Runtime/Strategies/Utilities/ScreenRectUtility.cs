// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Extensions;
using UnityEngine;

namespace TestHelper.UI.Strategies.Utilities
{
    /// <summary>
    /// Screen-space rect math used by <see cref="DefaultReachableStrategy"/> to pick fallback raycast points.
    /// </summary>
    internal static class ScreenRectUtility
    {
        private static readonly Vector3[] s_corners = new Vector3[4];

        /// <summary>
        /// Returns the axis-aligned screen-space bounds of the <c>RectTransform</c> of <paramref name="gameObject"/>,
        /// projected through its associated camera.
        /// </summary>
        /// <param name="gameObject">Target <c>GameObject</c></param>
        /// <param name="rect">Screen-space bounds; unspecified when the method returns false</param>
        /// <returns>False if <paramref name="gameObject"/> has no <c>RectTransform</c></returns>
        internal static bool TryGetScreenRect(GameObject gameObject, out Rect rect)
        {
            var rectTransform = gameObject.transform as RectTransform;
            if (rectTransform == null)
            {
                rect = default;
                return false;
            }

            rectTransform.GetWorldCorners(s_corners);
            var camera = gameObject.GetAssociatedCamera();
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var corner in s_corners)
            {
                var point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return true;
        }

        /// <summary>
        /// Returns the axis-aligned intersection of two rects.
        /// The result may have negative width or height when they are disjoint; callers treat under 1px as empty.
        /// </summary>
        internal static Rect Intersect(Rect a, Rect b)
        {
            return Rect.MinMaxRect(
                Mathf.Max(a.xMin, b.xMin),
                Mathf.Max(a.yMin, b.yMin),
                Mathf.Min(a.xMax, b.xMax),
                Mathf.Min(a.yMax, b.yMax));
        }

        /// <summary>
        /// Returns the largest strip of <paramref name="rect"/> that is not covered by <paramref name="blocker"/>.
        /// Subtracting an axis-aligned rect leaves at most four strips (left, right, bottom, top);
        /// ties are resolved in that order. A disjoint blocker returns <paramref name="rect"/> unchanged,
        /// and a blocker covering it returns a degenerate rect.
        /// </summary>
        internal static Rect LargestRemainder(Rect rect, Rect blocker)
        {
            var best = new Rect(rect.x, rect.y, 0, 0);
            var bestArea = 0f;

            // Strips are built from the rect's own bounds; using float.MinValue/MaxValue as open bounds
            // loses the blocker edge to floating-point rounding when Rect converts min/max to x/width.
            void Consider(float xMin, float yMin, float xMax, float yMax)
            {
                var width = xMax - xMin;
                var height = yMax - yMin;
                var area = Mathf.Max(0, width) * Mathf.Max(0, height);
                if (area > bestArea)
                {
                    best = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
                    bestArea = area;
                }
            }

            Consider(rect.xMin, rect.yMin, Mathf.Min(rect.xMax, blocker.xMin), rect.yMax); // left
            Consider(Mathf.Max(rect.xMin, blocker.xMax), rect.yMin, rect.xMax, rect.yMax); // right
            Consider(rect.xMin, rect.yMin, rect.xMax, Mathf.Min(rect.yMax, blocker.yMin)); // bottom
            Consider(rect.xMin, Mathf.Max(rect.yMin, blocker.yMax), rect.xMax, rect.yMax); // top
            return best;
        }
    }
}
