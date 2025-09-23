// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using TestHelper.UI.Strategies;
using UnityEngine;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Can customize the strategy function that determines the screen point of the <c>GameObject</c>.
    /// </summary>
    public interface IScreenPointCustomizable
    {
        /// <summary>
        /// Function that returns the screen position of <c>GameObject</c>.
        /// </summary>
        /// <seealso cref="DefaultScreenPointStrategy"/>
        Func<GameObject, Vector2> GetScreenPoint { set; }
    }
}
