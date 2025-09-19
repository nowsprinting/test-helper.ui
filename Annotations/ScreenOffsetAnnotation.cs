// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Annotations
{
    /// <summary>
    /// An annotation class that indicates the screen position offset where monkey operators operate.
    /// </summary>
    [AddComponentMenu("UI Test Helper/Screen Offset Annotation")]
    [HelpURL("https://nowsprinting.github.io/test-helper.ui/api/TestHelper.UI.Annotations.ScreenOffsetAnnotation.html")]
    public sealed class ScreenOffsetAnnotation : MonoBehaviour
    {
        /// <summary>
        /// Offset from a screen position of the <c>GameObject</c> that the annotation is attached to.
        /// Respects <c>CanvasScaler</c> but does not calculate the aspect ratio.
        /// </summary>
        [SerializeField]
        public Vector2 offset;
    }
}
