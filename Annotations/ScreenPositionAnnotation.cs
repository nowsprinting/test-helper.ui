// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Annotations
{
    /// <summary>
    /// An annotation class that indicates the screen position where monkey operators operate.
    /// </summary>
    [AddComponentMenu("UI Test Helper/Screen Position Annotation")]
    [HelpURL(
        "https://nowsprinting.github.io/test-helper.ui/api/TestHelper.UI.Annotations.ScreenPositionAnnotation.html")]
    public sealed class ScreenPositionAnnotation : MonoBehaviour
    {
        /// <summary>
        /// A screen position where monkey operators operate.
        /// It respects <c>CanvasScaler</c> but does not calculate the aspect ratio.
        /// </summary>
        [SerializeField]
        public Vector2 position;
    }
}
