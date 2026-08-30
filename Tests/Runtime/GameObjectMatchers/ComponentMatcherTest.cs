// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestHelper.UI.GameObjectMatchers
{
    [TestFixture]
    public class ComponentMatcherTest
    {
        [Test]
        [Category("Acceptance")]
        public void ComponentType_WithoutComponentType_ReturnsComponent()
        {
            var sut = new ComponentMatcher();
            Assert.That(sut.ComponentType, Is.EqualTo(typeof(Component)));
        }

        [Test]
        public void ComponentType_WithComponentType_ReturnsSpecifiedType()
        {
            var sut = new ComponentMatcher(componentType: typeof(Transform));
            Assert.That(sut.ComponentType, Is.EqualTo(typeof(Transform)));
        }

        [Test]
        public void ToString_WithoutArguments_ReturnsOnlyType()
        {
            var sut = new ComponentMatcher();
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.Component"));
        }

        [Test]
        public void ToString_WithComponentType_ReturnsWithComponentType()
        {
            var sut = new ComponentMatcher(componentType: typeof(Transform));
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.Transform"));
        }

        [Test]
        public void ToString_WithName_ReturnsWithName()
        {
            var sut = new ComponentMatcher(name: "component");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.Component, name=component"));
        }

        [Test]
        public void ToString_WithPath_ReturnsWithPath()
        {
            var sut = new ComponentMatcher(path: "/Path/To/Component");
            var actual = sut.ToString();
            Assert.That(actual, Is.EqualTo("type=UnityEngine.Component, path=/Path/To/Component"));
        }

        [Test]
        [CreateScene]
        public void IsMatch_NotMatchComponentType_ReturnsFalse()
        {
            var sut = new ComponentMatcher(componentType: typeof(Button)); // UnityEngine.UI.Button
            var actual = sut.IsMatch(new GameObject());                    // No Button component
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void IsMatch_NotMatchName_ReturnsFalse()
        {
            var sut = new ComponentMatcher(name: "transform");
            var actual = sut.IsMatch(CreateGameObject(name: "not_transform"));
            Assert.That(actual, Is.False);
        }

        [Test]
        [CreateScene]
        public void IsMatch_NotMatchPath_ReturnsFalse()
        {
            var sut = new ComponentMatcher(path: "/Path/To/Transform");
            var actual = sut.IsMatch(CreateGameObject(path: "/Path/To/Not/Transform"));
            Assert.That(actual, Is.False);
        }

        [Test]
        [Category("Acceptance")]
        [CreateScene]
        public void IsMatch_ComponentTypeIsInterface_ReturnsTrue()
        {
            var sut = new ComponentMatcher(componentType: typeof(IPointerClickHandler));
            var actual = sut.IsMatch(CreateGameObject(componentType: typeof(Button))); // Button implements IPointerClickHandler
            Assert.That(actual, Is.True);
        }

        [Test]
        [CreateScene]
        public void IsMatch_MatchAllProperties_ReturnsTrue()
        {
            var sut = new ComponentMatcher(
                componentType: typeof(Transform),
                name: "Transform",
                path: "/Path/??/Transform");
            var actual = sut.IsMatch(CreateGameObject(
                componentType: typeof(Transform),
                name: "Transform",
                path: "/Path/To/Transform"));
            Assert.That(actual, Is.True);
        }

        private static GameObject CreateGameObject(Type componentType = null, string name = null, string path = null)
        {
            var gameObject = new GameObject();

            if (path != null)
            {
                var enumerator = path.Split('/').Reverse().GetEnumerator();
                enumerator.MoveNext();
                gameObject.name = enumerator.Current ?? "null";
                var lastGameObject = gameObject;
                while (enumerator.MoveNext())
                {
                    var node = enumerator.Current;
                    if (string.IsNullOrEmpty(node))
                    {
                        continue;
                    }

                    var parent = new GameObject(node);
                    lastGameObject.transform.SetParent(parent.transform);
                    lastGameObject = parent;
                }

                enumerator.Dispose();
            }

            if (name != null)
            {
                gameObject.name = name; // Note: Allow it to be overwritten
            }

            if (componentType != null)
            {
                gameObject.AddComponent(componentType);
            }

            return gameObject;
        }
    }
}
