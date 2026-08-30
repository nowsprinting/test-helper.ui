// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Extensions
{
    /// <summary>
    /// Extension methods and version-bridging helpers for <see cref="UnityEngine.Object"/>.
    /// </summary>
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Returns the platform identity of the <see cref="UnityEngine.Object"/>.
        /// On Unity 6.4 or newer, returns <c>EntityId</c> via <c>GetEntityId()</c>;
        /// on older Unity, returns the <c>int</c> instance ID via <c>GetInstanceID()</c>.
        /// </summary>
        /// <param name="self">Target object</param>
        /// <returns>Platform identity token (EntityId on Unity 6.4+, int on older versions)</returns>
#if UNITY_6000_4_OR_NEWER
        internal static EntityId GetId(this Object self) => self.GetEntityId();
#else
        internal static int GetId(this Object self) => self.GetInstanceID();
#endif

        /// <summary>
        /// Finds all loaded objects of the specified type, excluding objects on inactive <c>GameObject</c>s.
        /// Result order is unspecified; on Unity 6.4 or newer the find APIs cannot sort at all.
        /// </summary>
        /// <param name="type">Type to find; must derive from <see cref="UnityEngine.Object"/></param>
        /// <returns>Found objects</returns>
        internal static Object[] FindObjectsByType(Type type)
        {
#if UNITY_6000_4_OR_NEWER
            return Object.FindObjectsByType(type, FindObjectsInactive.Exclude);
#elif UNITY_2022_3_OR_NEWER
            return Object.FindObjectsByType(type, FindObjectsSortMode.None);
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return Object.FindObjectsOfType(type);
#endif
        }

        /// <summary>
        /// Finds all loaded objects of the specified type, excluding objects on inactive <c>GameObject</c>s.
        /// Result order is unspecified; on Unity 6.4 or newer the find APIs cannot sort at all.
        /// </summary>
        /// <typeparam name="T">Type to find</typeparam>
        /// <returns>Found objects</returns>
        internal static T[] FindObjectsByType<T>() where T : Object
        {
#if UNITY_6000_4_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsInactive.Exclude);
#elif UNITY_2022_3_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            // Note: Supported in Unity 2020.3.4, 2021.3.18, 2022.2.5 or later.
#else
            return Object.FindObjectsOfType<T>();
#endif
        }
    }
}
