// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Extensions;
using UnityEditor;

namespace TestHelper.UI.Editor.ContextMenu
{
    public static class CopyToClipboardMenu
    {
        private const string Prefix = "GameObject/Copy to Clipboard/";

        [MenuItem(Prefix + "Hierarchy Path")]
        private static void CopyHierarchyPathMenuItem()
        {
            var selectedTransform = Selection.activeTransform;
            if (selectedTransform == null)
            {
                return;
            }

            var path = selectedTransform.GetPath();
            EditorGUIUtility.systemCopyBuffer = path;
        }

#if UNITY_6000_4_OR_NEWER
        [MenuItem(Prefix + "Entity ID")]
#else
        [MenuItem(Prefix + "Instance ID")]
#endif
        private static void CopyObjectIdMenuItem()
        {
            var selected = Selection.activeGameObject;
            if (selected == null)
            {
                return;
            }

            EditorGUIUtility.systemCopyBuffer = selected.GetId().ToString();
        }
    }
}
