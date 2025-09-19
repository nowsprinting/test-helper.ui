// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.Random;

namespace TestHelper.UI.Random
{
    /// <summary>
    /// Operators that behave randomly in monkey testing.
    /// </summary>
    public interface IRandomizable
    {
        /// <summary>
        /// Pseudo-random number generator instance.
        /// </summary>
        IRandom Random { set; }
    }
}
