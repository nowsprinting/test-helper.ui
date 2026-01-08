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

        /// <summary>
        /// The next fresh ID that has never been used.
        /// All IDs from 0 to _nextNewId-1 are either currently in use or in _releasedIds.
        /// </summary>
        private int _nextNewId;

        /// <summary>
        /// Released IDs that are available for reuse.
        /// Always contains IDs that are less than _nextNewId.
        /// Using SortedSet to get the minimum available ID in O(log n).
        /// </summary>
        // Reasons for using SortedSet instead of HashSet with linear search:
        // SortedSet.Min provides O(log n) access to the smallest available ID,
        // while the previous HashSet approach required O(n) linear scanning.
        private readonly SortedSet<int> _releasedIds = new SortedSet<int>();

        private FingerPool()
        {
        }

        /// <summary>
        /// Acquire the smallest available pointer ID.
        /// </summary>
        /// <returns>Available pointer ID</returns>
        public int Acquire()
        {
            if (_releasedIds.Count > 0)
            {
                var id = _releasedIds.Min;
                _releasedIds.Remove(id);
                return id;
            }

            return _nextNewId++;
        }

        /// <summary>
        /// Release the specified pointer ID.
        /// </summary>
        /// <param name="id">Pointer ID to release</param>
        public void Release(int id)
        {
            if (id < 0 || id >= _nextNewId)
            {
                return;
            }

            if (id == _nextNewId - 1)
            {
                _nextNewId--;
                while (_releasedIds.Count > 0 && _releasedIds.Max == _nextNewId - 1)
                {
                    _releasedIds.Remove(_releasedIds.Max);
                    _nextNewId--;
                }
            }
            else
            {
                _releasedIds.Add(id);
            }
        }

        /// <summary>
        /// Reset the pool state. For testing purposes.
        /// </summary>
        public void Reset()
        {
            _nextNewId = 0;
            _releasedIds.Clear();
        }
    }
}
