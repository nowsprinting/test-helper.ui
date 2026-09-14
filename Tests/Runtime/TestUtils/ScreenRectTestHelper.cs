// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Extensions;
using UnityEngine;

namespace TestHelper.UI.TestUtils
{
    // Test-side oracle for screen-space rects; the production ScreenRectUtility is the subject under test.
    internal static class ScreenRectTestHelper
    {
        internal static Rect GetScreenRect(GameObject gameObject)
        {
            var corners = new Vector3[4];
            ((RectTransform)gameObject.transform).GetWorldCorners(corners);
            var camera = gameObject.GetAssociatedCamera();
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var corner in corners)
            {
                var point = RectTransformUtility.WorldToScreenPoint(camera, corner);
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
