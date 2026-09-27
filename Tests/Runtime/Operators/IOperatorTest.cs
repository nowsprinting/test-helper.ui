// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.TestTools;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Checks that every built-in operator is defined so that <see cref="OperatorPool"/> can create it.
    /// </summary>
    [TestFixture]
    public class IOperatorTest
    {
        private static IEnumerable<Type> BuiltInOperatorTypes()
        {
            return typeof(IOperator).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && typeof(IOperator).IsAssignableFrom(type));
        }

        [Test]
        public void Rent_BuiltInOperatorWithoutArgs_ReturnsInstanceOfThatType(
            [ValueSource(nameof(BuiltInOperatorTypes))] Type type)
        {
            // Registering each type with Register<T>() is rejected: a built-in operator added later would stay
            // untested until its registration is added here, while this pool rents any type without registration.
            var pool = new OperatorPool(requireRegistration: false);

            Assert.That(pool.Rent(type), Is.TypeOf(type));
        }

        [Test]
        // Running on the Player is rejected: managed code stripping removes a constructor without [Preserve],
        // so reflection there does not list it and the check passes for the very constructor it looks for.
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public void PublicConstructors_BuiltInOperator_ArePreserved(
            [ValueSource(nameof(BuiltInOperatorTypes))] Type type)
        {
            var isTypePreserved = type.IsDefined(typeof(PreserveAttribute), false);
            var notPreserved = type.GetConstructors()
                .Where(constructor => !constructor.IsDefined(typeof(PreserveAttribute), false) &&
                                      !(isTypePreserved && constructor.GetParameters().Length == 0))
                .Select(constructor => constructor.ToString());

            Assert.That(notPreserved, Is.Empty);
        }

        [Test]
        // Running on the Player is rejected: managed code stripping removes the implementation of a sub-interface
        // that no code uses, so reflection there does not show the interfaces declared in the source.
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public void Interfaces_BuiltInOperator_ContainsSubInterfaceOfIOperator(
            [ValueSource(nameof(BuiltInOperatorTypes))] Type type)
        {
            var subInterfaces = type.GetInterfaces()
                .Where(implemented => implemented != typeof(IOperator) &&
                                      typeof(IOperator).IsAssignableFrom(implemented));

            Assert.That(subInterfaces, Is.Not.Empty);
        }
    }
}
