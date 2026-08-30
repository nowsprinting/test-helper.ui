// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Exceptions;
using TestHelper.UI.Extensions;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Paginators;
using TestHelper.UI.Strategies;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI
{
    /// <summary>
    /// Find <c>GameObject</c> by name or path (glob). Wait until they appear.
    /// </summary>
    public class GameObjectFinder
    {
        private readonly double _timeoutSeconds;
        private readonly IReachableStrategy _reachableStrategy;
        private readonly Func<Component, bool> _isInteractable;
        private readonly IVisualizer _visualizer;

        private const double MinTimeoutSeconds = 0.01d;
        private const double MaxPollingIntervalSeconds = 1.0d;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appears.</param>
        /// <param name="reachableStrategy">Strategy to examine whether <c>GameObject</c> is reachable from the user. Default is <c>DefaultReachableStrategy</c>.</param>
        /// <param name="isInteractable">The function returns the <c>Component</c> is interactable or not. Default is <c>DefaultComponentInteractableStrategy.IsInteractable</c>.</param>
        /// <param name="visualizer">Visualizer set if you need to show fault indicators.</param>
        public GameObjectFinder(double timeoutSeconds = 1.0d,
            IReachableStrategy reachableStrategy = null,
            Func<Component, bool> isInteractable = null,
            IVisualizer visualizer = null)
        {
            if (timeoutSeconds < MinTimeoutSeconds)
            {
                throw new ArgumentException(
                    $"Must be greater than or equal to {MinTimeoutSeconds.ToString(CultureInfo.InvariantCulture)}.",
                    nameof(timeoutSeconds));
            }

            _timeoutSeconds = timeoutSeconds;
            _reachableStrategy = reachableStrategy ?? new DefaultReachableStrategy();
            _isInteractable = isInteractable ?? DefaultComponentInteractableStrategy.IsInteractable;
            _visualizer = visualizer;
        }

        private enum Reason
        {
            NotFound,
            NotReachable,
            NotInteractable,
            MultipleMatching,
            None
        }

        private bool FilterToOnlyReachable(ref List<GameObject> objects,
            out Dictionary<GameObject, RaycastResult> raycastResults)
        {
            raycastResults = new Dictionary<GameObject, RaycastResult>(objects.Count);
            for (var i = objects.Count - 1; i >= 0; i--)
            {
                var current = objects[i];
                if (_reachableStrategy.IsReachable(current, out var raycastResult))
                {
                    raycastResults.Add(current, raycastResult);
                    continue;
                }

                _visualizer?.ShowNotReachableIndicator(raycastResult.screenPosition, raycastResult.gameObject);
                objects.RemoveAt(i);
            }

            return objects.Count > 0;
        }

        private bool FilterToOnlyInteractable(ref List<GameObject> objects)
        {
            for (var i = objects.Count - 1; i >= 0; i--)
            {
                var current = objects[i];
                if (HasInteractableComponent(current))
                {
                    continue;
                }

                _visualizer?.ShowNotInteractableIndicator(current);
                objects.RemoveAt(i);
            }

            return objects.Count > 0;
        }

        private bool HasInteractableComponent(GameObject gameObject)
        {
            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (_isInteractable(component))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<GameObject> FindAllByMatcher(IGameObjectMatcher matcher)
        {
            var componentType = matcher.ComponentType;
            if (componentType == null || componentType == typeof(Component) ||
                !typeof(Component).IsAssignableFrom(componentType))
            {
                // Transform yields exactly one hit per GameObject, so remap types the native query
                // handles poorly: null (a custom matcher without a hint), typeof(Component) (returns
                // every component instance in the scene), and non-Object-derived types such as
                // interfaces (rejected by FindObjectsByType). matcher.IsMatch still applies the
                // matcher's own criteria either way.
                componentType = typeof(Transform);
            }

            var components = ObjectExtensions.FindObjectsByType(componentType);

            // Dedupe by GameObject: FindObjectsByType returns one hit per component instance,
            // so a GameObject holding multiple matching components would otherwise be judged
            // as multiple matches.
            var seenObjects = new HashSet<GameObject>();
            var foundObjects = new List<GameObject>();
            foreach (var component in components)
            {
                var gameObject = ((Component)component).gameObject;
                if (seenObjects.Add(gameObject) && matcher.IsMatch(gameObject))
                {
                    foundObjects.Add(gameObject);
                }
            }

            return foundObjects;
        }

        private (GameObject, RaycastResult, Reason) FindByMatcher(IGameObjectMatcher matcher,
            bool reachable, bool interactable)
        {
            var foundObjects = FindAllByMatcher(matcher);
            if (foundObjects.Count == 0)
            {
                return (null, default, Reason.NotFound);
            }

            Dictionary<GameObject, RaycastResult> raycastResults = null;
            if (reachable && !FilterToOnlyReachable(ref foundObjects, out raycastResults))
            {
                return (null, default, Reason.NotReachable);
            }

            if (interactable && !FilterToOnlyInteractable(ref foundObjects))
            {
                return (null, default, Reason.NotInteractable);
            }

            if (foundObjects.Count > 1)
            {
                return (null, default, Reason.MultipleMatching);
            }

            // Reuse the raycast captured while filtering; raycasting the survivor again would
            // repeat the most expensive step of the poll for an identical same-frame result.
            var resultObject = foundObjects[0];
            var raycastResult = reachable ? raycastResults[resultObject] : new RaycastResult();
            return (resultObject, raycastResult, Reason.None);
        }

        private async UniTask<(GameObject, RaycastResult, Reason)> FindInPaginatorAsync(
            IGameObjectMatcher matcher,
            bool reachable,
            bool interactable,
            IPaginator paginator,
            CancellationToken cancellationToken)
        {
            await paginator.ResetAsync(cancellationToken);

            var lastMeaningfulReason = Reason.NotFound;
            var nextPage = true;

            while (nextPage)
            {
                // FindByMatcher is a synchronous scene snapshot, not a blocking variant of
                // FindByMatcherAsync; awaiting the async method here would recurse into the
                // polling wrapper that calls this method.
#pragma warning disable VSTHRD103
                var (foundObject, raycastResult, reason) = FindByMatcher(matcher, reachable, interactable);
#pragma warning restore VSTHRD103

                if (foundObject != null)
                {
                    return (foundObject, raycastResult, reason);
                }

                if (reason != Reason.NotFound)
                {
                    lastMeaningfulReason = reason;
                }

                nextPage = await paginator.NextPageAsync(cancellationToken);
            }

            return (null, default, lastMeaningfulReason);
        }

        /// <summary>
        /// Find <c>GameObject</c> by <see cref="IGameObjectMatcher"/> (wait until they appear).
        /// </summary>
        /// <param name="matcher"></param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="paginator">Pagination controller for finding <c>GameObject</c> on pageable (or scrollable) UI components (e.g., Scroll view, Carousel, Paged dialog).</param>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appears. This parameter is respected over the same-name constructor argument. If omitted, use the constructor argument value.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found <c>GameObject</c> and the frontmost raycast hit result will be set regardless of whether the event can be processed</returns>
        /// <exception cref="TimeoutException">Throws if <c>GameObject</c> is not found</exception>
        public async UniTask<GameObjectFinderResult> FindByMatcherAsync(IGameObjectMatcher matcher,
            bool reachable = true, bool interactable = false, IPaginator paginator = null, double timeoutSeconds = 0,
            CancellationToken cancellationToken = default)
        {
            var timeoutTime = Time.realtimeSinceStartup +
                              (timeoutSeconds >= MinTimeoutSeconds ? timeoutSeconds : _timeoutSeconds);
            var delaySeconds = MinTimeoutSeconds;
            var reason = Reason.None;

            while (Time.realtimeSinceStartup < timeoutTime)
            {
                GameObject foundObject;
                RaycastResult raycastResult;

                if (paginator != null)
                {
                    (foundObject, raycastResult, reason) = await FindInPaginatorAsync(matcher, reachable, interactable,
                        paginator, cancellationToken);
                }
                else
                {
                    (foundObject, raycastResult, reason) = FindByMatcher(matcher, reachable, interactable);
                }

                if (foundObject)
                {
                    return new GameObjectFinderResult(foundObject, raycastResult);
                }

                delaySeconds = Math.Min(delaySeconds * 2, MaxPollingIntervalSeconds);
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), ignoreTimeScale: true,
                    cancellationToken: cancellationToken);
            }

            switch (reason)
            {
                case Reason.NotFound:
                    throw new TimeoutException($"GameObject ({matcher}) is not found.");
                case Reason.NotReachable:
                    throw new TimeoutException($"GameObject ({matcher}) is found, but not reachable.");
                case Reason.NotInteractable:
                    throw new TimeoutException($"GameObject ({matcher}) is found, but not interactable.");
                case Reason.MultipleMatching:
                    throw new MultipleGameObjectsMatchingException(
                        $"Multiple GameObjects matching the condition ({matcher}) were found.");
                case Reason.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Find <c>GameObject</c> by name (wait until they appear).
        /// </summary>
        /// <remarks>
        /// When you can identify the component type, using <see cref="FindByMatcherAsync"/> is more advantageous in both execution speed and memory usage.
        /// </remarks>
        /// <param name="name">Find <c>GameObject</c> name</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="paginator">Pagination controller for finding <c>GameObject</c> on pageable (or scrollable) UI components (e.g., Scroll view, Carousel, Paged dialog).</param>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appears. This parameter is respected over the same-name constructor argument. If omitted, use the constructor argument value.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found <c>GameObject</c> and the frontmost raycast hit result will be set regardless of whether the event can be processed</returns>
        /// <exception cref="TimeoutException">Throws if <c>GameObject</c> is not found</exception>
        public UniTask<GameObjectFinderResult> FindByNameAsync(string name, bool reachable = true,
            bool interactable = false, IPaginator paginator = null, double timeoutSeconds = 0,
            CancellationToken cancellationToken = default)
        {
            var matcher = new NameMatcher(name);
            return FindByMatcherAsync(matcher, reachable, interactable, paginator, timeoutSeconds,
                cancellationToken);
        }

        /// <summary>
        /// Find <c>GameObject</c> by path (wait until they appear).
        /// </summary>
        /// <remarks>
        /// When you can identify the component type, using <see cref="FindByMatcherAsync"/> is more advantageous in both execution speed and memory usage.
        /// </remarks>
        /// <param name="path">Find <c>GameObject</c> hierarchy path separated by `/`. Can specify wildcards of glob pattern (`?`, `*`, and `**`).</param>
        /// <param name="reachable">Find only reachable object</param>
        /// <param name="interactable">Find only interactable object</param>
        /// <param name="paginator">Pagination controller for finding <c>GameObject</c> on pageable (or scrollable) UI components (e.g., Scroll view, Carousel, Paged dialog).</param>
        /// <param name="timeoutSeconds">Seconds to wait until <c>GameObject</c> appears. This parameter is respected over the same-name constructor argument. If omitted, use the constructor argument value.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Found <c>GameObject</c> and the frontmost raycast hit result will be set regardless of whether the event can be processed</returns>
        /// <exception cref="TimeoutException">Throws if <c>GameObject</c> is not found</exception>
        /// <seealso href="https://en.wikipedia.org/wiki/Glob_(programming)"/>
        public UniTask<GameObjectFinderResult> FindByPathAsync(string path, bool reachable = true,
            bool interactable = false, IPaginator paginator = null, double timeoutSeconds = 0,
            CancellationToken cancellationToken = default)
        {
            var matcher = new PathMatcher(path);
            return FindByMatcherAsync(matcher, reachable, interactable, paginator, timeoutSeconds,
                cancellationToken);
        }
    }
}
