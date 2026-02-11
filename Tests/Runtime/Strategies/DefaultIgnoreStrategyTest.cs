// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.Annotations;
using TestHelper.UI.GameObjectMatchers;
using UnityEngine;

namespace TestHelper.UI.Strategies
{
    [TestFixture]
    public class DefaultIgnoreStrategyTest
    {
        private const string TestScene = "../../Scenes/PhysicsRaycasterSandbox.unity";

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithAnnotation_Ignored()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<IgnoreAnnotation>();

            Assert.That(new DefaultIgnoreStrategy().IsIgnored(cube), Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithDisabledAnnotation_NotIgnored()
        {
            var cube = GameObject.Find("Cube");
            var annotation = cube.AddComponent<IgnoreAnnotation>();
            annotation.enabled = false;

            Assert.That(new DefaultIgnoreStrategy().IsIgnored(cube), Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithoutAnnotation_NotIgnored()
        {
            var cube = GameObject.Find("Cube");
            Assert.That(new DefaultIgnoreStrategy().IsIgnored(cube), Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_ChildOfAnnotatedObject_Ignored()
        {
            var cube = GameObject.Find("Cube");
            cube.AddComponent<IgnoreAnnotation>();
            var child = new GameObject("Child");
            child.transform.SetParent(cube.transform);

            Assert.That(new DefaultIgnoreStrategy().IsIgnored(child), Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_ChildOfDisabledAnnotatedObject_NotIgnored()
        {
            var cube = GameObject.Find("Cube");
            var annotation = cube.AddComponent<IgnoreAnnotation>();
            annotation.enabled = false;
            var child = new GameObject("Child");
            child.transform.SetParent(cube.transform);

            Assert.That(new DefaultIgnoreStrategy().IsIgnored(child), Is.False);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_WithIgnoreMatchers_Ignored()
        {
            var cube = GameObject.Find("Cube");
            var ignoreMatchers = new List<IGameObjectMatcher> { new NameMatcher("Cube") };
            var sut = new DefaultIgnoreStrategy(ignoreMatchers: ignoreMatchers);

            Assert.That(sut.IsIgnored(cube), Is.True);
        }

        [Test]
        [LoadScene(TestScene)]
        public void IsIgnored_ChildOfMatchedObject_Ignored()
        {
            var cube = GameObject.Find("Cube");
            var child = new GameObject("Child");
            child.transform.SetParent(cube.transform);
            var ignoreMatchers = new List<IGameObjectMatcher> { new NameMatcher("Cube") };
            var sut = new DefaultIgnoreStrategy(ignoreMatchers: ignoreMatchers);

            Assert.That(sut.IsIgnored(child), Is.True);
        }
    }
}
