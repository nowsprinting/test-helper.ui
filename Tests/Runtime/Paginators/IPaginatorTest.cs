// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace TestHelper.UI.Paginators
{
    [TestFixture]
    public class IPaginatorTest
    {
        private static Type[] GetPaginators()
        {
            return TypeCache.GetTypesDerivedFrom<IPaginator>().ToArray();
        }

        /// <summary>
        /// Verify that the paginators and supported component type can be obtained via reflection.
        /// </summary>
        [TestCaseSource(nameof(GetPaginators))]
        public void Constructor_HasOneParameterAndSubclassOfMonoBehaviour(Type paginatorType)
        {
            var ctor = paginatorType.GetConstructors()
                .OrderBy(x => x.GetParameters().Length)
                .FirstOrDefault(x => x.GetParameters().Length > 0);
            Assume.That(ctor, Is.Not.Null, "A paginator must have a constructor with one or more parameters.");

            var parameterType = ctor.GetParameters()[0].ParameterType;
            Assert.That(parameterType.IsSubclassOf(typeof(MonoBehaviour)), Is.True,
                "The first parameter of the constructor is a pageable or scrollable component to be controlled, which must be a subclass of MonoBehaviour.");
        }
    }
}
