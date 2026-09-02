// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
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
                .ToArray();
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
