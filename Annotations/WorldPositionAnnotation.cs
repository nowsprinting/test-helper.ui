// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Annotations
{
    /// <summary>
    /// An annotation class that indicate the world position that where monkey operators operate on.
    /// </summary>
    [AddComponentMenu("UI Test Helper/World Position Annotation")]
    [HelpURL(
        "https://nowsprinting.github.io/test-helper.ui/api/TestHelper.UI.Annotations.WorldPositionAnnotation.html")]
    public sealed class WorldPositionAnnotation : MonoBehaviour
    {
        /// <summary>
        /// A world position that where monkey operators operate on
        /// </summary>
        [SerializeField]
        public Vector3 position;
    }
}
