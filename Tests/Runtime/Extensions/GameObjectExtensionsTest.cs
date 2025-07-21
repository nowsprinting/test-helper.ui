// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Monkey.TestDoubles;
using UnityEngine;
using UnityEngine.UI;
#if !UNITY_2023_1_OR_NEWER
using System.Linq;
using Cysharp.Threading.Tasks;
#endif

namespace TestHelper.Monkey.Extensions
{
    [TestFixture]
    public class GameObjectExtensionsTest
    {
        [Test]
        public void GetInteractableComponents_GotInteractableComponents()
        {
            var gameObject = new GameObject();
            var onPointerClickHandler = gameObject.AddComponent<SpyOnPointerClickHandler>();
            var onPointerDownUpHandler = gameObject.AddComponent<SpyOnPointerDownUpHandler>();
            gameObject.AddComponent<Image>(); // Not interactable

            var actual = gameObject.GetInteractableComponents();
            Assert.That(actual, Is.EquivalentTo(new Component[] { onPointerClickHandler, onPointerDownUpHandler }));
        }

        [Test]
        public void GetInteractableComponents_NoInteractableComponents_ReturnsEmpty()
        {
            var button = new GameObject().AddComponent<Button>();
            button.interactable = false;

            var actual = button.gameObject.GetInteractableComponents();
            Assert.That(actual, Is.Empty);
        }
    }
}
