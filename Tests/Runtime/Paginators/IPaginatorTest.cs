// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.Scripting;
#if UNITY_6000_4_OR_NEWER
using UnityEngine.Assemblies;
#endif

namespace TestHelper.UI.Paginators
{
    [TestFixture]
    public class IPaginatorTest
    {
        private static Type[] GetPaginators()
        {
            var interfaceType = typeof(IPaginator);
#if UNITY_6000_4_OR_NEWER
            return CurrentAssemblies.GetLoadedAssemblies()
#else
            return AppDomain.CurrentDomain.GetAssemblies()
#endif
                .SelectMany(assembly =>
                {
                    try
                    {
                        return assembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                    catch
                    {
                        return Enumerable.Empty<Type>();
                    }
                })
                .Where(t => t != null && interfaceType.IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
                // Test doubles are excluded because some of them intentionally violate the constructor rules.
                .Where(t => t.Namespace != "TestHelper.UI.TestDoubles")
                .ToArray();
        }

        [TestCaseSource(nameof(GetPaginators))]
        public void Constructor_RentedFromPoolWithoutRegistration_ReturnsInstanceOfPaginatorType(Type paginatorType)
        {
            var pool = new PaginatorPool(requireRegistration: false);

            var actual = pool.Rent(paginatorType);

            Assert.That(actual, Is.InstanceOf(paginatorType));
        }

        [TestCaseSource(nameof(GetPaginators))]
        public void Constructor_PublicConstructors_HavePreserveAttribute(Type paginatorType)
        {
            var constructors = paginatorType.GetConstructors();

            Assert.That(constructors, Has.All.Matches<ConstructorInfo>(x => x.IsDefined(typeof(PreserveAttribute))));
        }
    }
}
