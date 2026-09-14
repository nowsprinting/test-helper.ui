// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Strategies.Utilities
{
    /// <summary>
    /// Screen-space rect math used by <see cref="DefaultReachableStrategy"/> to pick fallback raycast points.
    /// </summary>
    internal static class ScreenRectUtility
    {
        /// <summary>
        /// Returns the axis-aligned screen-space bounds of the <c>RectTransform</c> of <paramref name="gameObject"/>,
        /// projected through its associated camera.
        /// </summary>
        /// <param name="gameObject">Target <c>GameObject</c></param>
        /// <param name="rect">Screen-space bounds; unspecified when the method returns false</param>
        /// <returns>False if <paramref name="gameObject"/> has no <c>RectTransform</c></returns>
        internal static bool TryGetScreenRect(GameObject gameObject, out Rect rect)
        {
            rect = default;
            return false;
        }

        /// <summary>
        /// Returns the axis-aligned intersection of two rects.
        /// The result may have negative width or height when they are disjoint; callers treat under 1px as empty.
        /// </summary>
        internal static Rect Intersect(Rect a, Rect b)
        {
            return default;
        }

        /// <summary>
        /// Returns the largest strip of <paramref name="rect"/> that is not covered by <paramref name="blocker"/>.
        /// Subtracting an axis-aligned rect leaves at most four strips (left, right, bottom, top);
        /// ties are resolved in that order. A disjoint blocker returns <paramref name="rect"/> unchanged,
        /// and a blocker covering it returns a degenerate rect.
        /// </summary>
        internal static Rect LargestRemainder(Rect rect, Rect blocker)
        {
            return default;
        }
    }
}
