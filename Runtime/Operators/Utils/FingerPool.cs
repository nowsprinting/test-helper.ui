// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;

namespace TestHelper.UI.Operators.Utils
{
    /// <summary>
    /// Singleton class that manages pointer IDs for touch screen input.
    /// </summary>
    internal sealed class FingerPool
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static FingerPool Instance { get; } = new FingerPool();

        private readonly HashSet<int> _usedIds = new HashSet<int>();

        private FingerPool()
        {
        }

        /// <summary>
        /// Acquire the smallest available pointer ID.
        /// </summary>
        /// <returns>Available pointer ID</returns>
        public int Acquire()
        {
            var id = 0;
            while (_usedIds.Contains(id))
            {
                id++;
            }

            _usedIds.Add(id);
            return id;
        }

        /// <summary>
        /// Release the specified pointer ID.
        /// </summary>
        /// <param name="id">Pointer ID to release</param>
        public void Release(int id)
        {
            _usedIds.Remove(id);
        }

        /// <summary>
        /// Reset the pool state. For testing purposes.
        /// </summary>
        public void Reset()
        {
            _usedIds.Clear();
        }
    }
}
