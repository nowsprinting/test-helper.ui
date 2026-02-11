// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using TestHelper.UI.Annotations;
using TestHelper.UI.Extensions;
using TestHelper.UI.GameObjectMatchers;
using UnityEngine;

namespace TestHelper.UI.Strategies
{
    /// <summary>
    /// Default strategy to examine whether <c>GameObject</c> should be ignored.
    /// </summary>
    public class DefaultIgnoreStrategy : IIgnoreStrategy
    {
        private readonly ILogger _verboseLogger;
        private readonly List<IGameObjectMatcher> _ignoreMatchers;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <param name="ignoreMatchers">List of matchers to identify GameObjects (and their children) that should be ignored</param>
        public DefaultIgnoreStrategy(ILogger verboseLogger = null, List<IGameObjectMatcher> ignoreMatchers = null)
        {
            _verboseLogger = verboseLogger;
            _ignoreMatchers = ignoreMatchers;
        }

        /// <summary>
        /// Returns whether the <c>GameObject</c> is ignored or not.
        /// Default implementation checks whether the <c>GameObject</c> or any of its ancestors has an enabled <c>IgnoreAnnotation</c> component,
        /// or matches any of the configured ignore matchers (including their children).
        /// </summary>
        /// <param name="gameObject">Target <c>GameObject</c></param>
        /// <param name="verboseLogger">Logger set if you need verbose output</param>
        /// <returns>True if <c>GameObject</c> is ignored</returns>
        public bool IsIgnored(GameObject gameObject, ILogger verboseLogger = null)
        {
            verboseLogger = verboseLogger ?? _verboseLogger; // If null, use the specified in the constructor.

            var isIgnored = HasEnabledIgnoreAnnotationInParent(gameObject) ||
                            IsMatchedOrChildOfIgnoreMatchersMatched(gameObject);
            if (isIgnored && verboseLogger != null)
            {
                verboseLogger.Log($"Ignored {gameObject.name}({gameObject.GetInstanceID()}).");
            }

            return isIgnored;
        }

        private bool HasEnabledIgnoreAnnotationInParent(GameObject gameObject)
        {
            // TODO: Implement parent-chain traversal
            return false;
        }

        private bool IsMatchedOrChildOfIgnoreMatchersMatched(GameObject gameObject)
        {
            // TODO: Implement matcher checking with parent-chain traversal
            return false;
        }
    }
}
