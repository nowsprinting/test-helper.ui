// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.Strategies;

namespace TestHelper.UI.Operators
{
    /// <summary>
    /// Can customize the strategy to determine whether <c>GameObject</c> is reachable from the user.
    /// </summary>
    public interface IReachableStrategyCustomizable
    {
        /// <summary>
        /// Strategy to examine whether <c>GameObject</c> is reachable from the user.
        /// </summary>
        /// <seealso cref="DefaultReachableStrategy"/>
        IReachableStrategy ReachableStrategy { set; }
    }
}
