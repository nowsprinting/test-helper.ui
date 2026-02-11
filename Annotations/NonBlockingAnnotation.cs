// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Annotations
{
    /// <summary>
    /// Does not block reachability of other objects in <c>DefaultReachableStrategy.IsReachable</c>.
    /// However, the annotated <c>GameObject</c> itself and its child objects are reachable if it is the target.
    /// </summary>
    [AddComponentMenu("UI Test Helper/Non Blocking Annotation")]
    [HelpURL("https://nowsprinting.github.io/test-helper.ui/api/TestHelper.UI.Annotations.NonBlockingAnnotation.html")]
    public class NonBlockingAnnotation : MonoBehaviour
    {
    }
}
