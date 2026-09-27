// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;

namespace TestHelper.UI.Paginators
{
    /// <summary>
    /// Checks that every built-in paginator is defined so that <see cref="PaginatorPool"/> can create and select it.
    /// </summary>
    [TestFixture]
    public class IPaginatorTest
    {
        private static IEnumerable<Type> BuiltInPaginatorTypes()
        {
            return typeof(IPaginator).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IPaginator).IsAssignableFrom(type));
        }

        [Test]
        public void Rent_BuiltInPaginatorWithoutArgs_ReturnsInstanceOfThatType(
            [ValueSource(nameof(BuiltInPaginatorTypes))] Type type)
        {
            // Registering each type with Register<T>() is rejected: a built-in paginator added later would stay
            // untested until its registration is added here, while this pool rents any type without registration.
            var pool = new PaginatorPool(requireRegistration: false);

            Assert.That(pool.Rent(type), Is.TypeOf(type));
        }

        [Test]
        // Running on the Player is rejected: managed code stripping removes a constructor without [Preserve],
        // so reflection there does not list it and the check passes for the very constructor it looks for.
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public void PublicConstructors_BuiltInPaginator_ArePreserved(
            [ValueSource(nameof(BuiltInPaginatorTypes))] Type type)
        {
            var isTypePreserved = type.IsDefined(typeof(PreserveAttribute), false);
            var notPreserved = type.GetConstructors()
                .Where(constructor => !constructor.IsDefined(typeof(PreserveAttribute), false) &&
                                      !(isTypePreserved && constructor.GetParameters().Length == 0))
                .Select(constructor => constructor.ToString());

            Assert.That(notPreserved, Is.Empty);
        }

        [Test]
        // Running on the Player is rejected: whether managed code stripping keeps the IPaginator<TComponent>
        // implementation depends on the code that uses it, not on the declaration this test checks.
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public void Interfaces_BuiltInPaginator_ContainsGenericIPaginator(
            [ValueSource(nameof(BuiltInPaginatorTypes))] Type type)
        {
            var genericPaginators = type.GetInterfaces()
                .Where(implemented => implemented.IsGenericType &&
                                      implemented.GetGenericTypeDefinition() == typeof(IPaginator<>));

            Assert.That(genericPaginators, Is.Not.Empty);
        }
    }
}
