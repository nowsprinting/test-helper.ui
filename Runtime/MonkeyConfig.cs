// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using TestHelper.Random;
using TestHelper.UI.Exceptions;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using UnityEngine;

namespace TestHelper.UI
{
    /// <summary>
    /// Run configuration for monkey testing
    /// </summary>
    public class MonkeyConfig
    {
        /// <summary>
        /// Running time
        /// </summary>
        public TimeSpan Lifetime { get; set; } = new TimeSpan(0, 0, 1, 0); // 1min

        /// <summary>
        /// Delay time between operations
        /// </summary>
        public int DelayMillis { get; set; } = 200;

        /// <summary>
        /// Seconds after which a <see cref="TimeoutException"/> is thrown if no interactive component is found.
        /// Disable detection if set to 0.
        /// </summary>
        public int SecondsToErrorForNoInteractiveComponent { get; set; } = 5;

        /// <summary>
        /// An <see cref="InfiniteLoopException"/> is thrown if a repeating operation is detected within the specified buffer length.
        /// For example, if the buffer length is 10, repeating 5-step sequences can be detected.
        /// Disable detection if set to 0.
        /// </summary>
        public int BufferLengthForDetectLooping { get; set; } = 10;

        /// <summary>
        /// Pseudo-random number generator
        /// </summary>
        public IRandom Random { get; set; } = new RandomWrapper();

        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger { get; set; } = Debug.unityLogger;

        /// <summary>
        /// Output verbose log if true
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// Show Gizmos on <c>GameView</c> during running monkey test if true
        /// </summary>
        [Obsolete("Use GizmosShowOnGameViewAttribute or GameViewControlHelper.SetGizmos instead.")]
        public bool Gizmos { get; set; }

        /// <summary>
        /// Take screenshots during running the monkey test if set a <c>ScreenshotOptions</c> instance.
        /// </summary>
        public ScreenshotOptions Screenshots { get; set; }

        /// <summary>
        /// Function returns the <c>Component</c> is interactable or not.
        /// </summary>
        public Func<Component, bool> IsInteractable { get; set; } = DefaultComponentInteractableStrategy.IsInteractable;

        /// <summary>
        /// Strategy to examine whether <c>GameObject</c> should be ignored.
        /// <c>verboseLogger</c> will be overridden at runtime by the <c>Logger</c> if <c>Verbose</c> is true.
        /// </summary>
        /// <remarks>
        /// This strategy replaces the v0.14.0 or older <c>IsIgnore</c> function.
        /// </remarks>
        public IIgnoreStrategy IgnoreStrategy { get; set; } = new DefaultIgnoreStrategy();

        /// <summary>
        /// Strategy to get the screen position of the <c>GameObject</c>.
        /// </summary>
        public Func<GameObject, Vector2> GetScreenPoint { get; set; } = DefaultScreenPointStrategy.GetScreenPoint;

        /// <summary>
        /// Strategy to examine whether <c>GameObject</c> is reachable from the user.
        /// <c>verboseLogger</c> will be overridden at runtime by the <c>Logger</c> if <c>Verbose</c> is true.
        /// </summary>
        /// <remarks>
        /// This strategy replaces the v0.14.0 or older <c>IsReachable</c> function.
        /// </remarks>
        public IReachableStrategy ReachableStrategy { get; set; } = new DefaultReachableStrategy();

        /// <summary>
        /// Operators that the monkey invokes.
        /// <p/>
        /// <c>logger</c>, <c>screenshotOptions</c>, <c>GetScreenPoint</c>, and <c>ReachableStrategy</c> will be overridden at runtime by the same name properties in this <c>MonkeyConfig</c> instance.
        /// And <c>Random</c> will also be overridden at runtime by a forked instance from the <c>Random</c> property in this <c>MonkeyConfig</c> instance.
        /// </summary>
        public IEnumerable<IOperator> Operators { get; set; } = new IOperator[]
        {
            new UguiClickOperator(),
            new UguiTextInputOperator(),
        };
    }
}
