// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TestHelper.UI.Editor.Validators
{
    [TestFixture]
    public class SceneValidator
    {
        private static IEnumerable<TestCaseData> Scenes => AssetDatabase
            .FindAssets("t:SceneAsset", new string[] { "Packages/com.nowsprinting.test-helper.ui" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => new TestCaseData(path).SetName(Path.GetFileName(path)));

        private static void AssertMissingScriptRecursive(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();
            foreach (var component in components)
            {
                Assert.That(component, Is.Not.Null, $"Component in {gameObject.name}");
            }

            foreach (Transform child in gameObject.transform)
            {
                AssertMissingScriptRecursive(child.gameObject);
            }
        }

        [TestCaseSource(nameof(Scenes))]
        public void NoMissingScript(string path)
        {
            var scene = EditorSceneManager.OpenScene(path);
            foreach (var root in scene.GetRootGameObjects())
            {
                AssertMissingScriptRecursive(root);
            }
        }
    }
}
