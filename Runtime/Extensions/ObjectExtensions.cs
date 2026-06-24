// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="UnityEngine.Object"/>.
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
    }
}
