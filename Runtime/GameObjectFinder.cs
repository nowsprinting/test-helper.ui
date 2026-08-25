// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Exceptions;
using TestHelper.UI.GameObjectMatchers;
using TestHelper.UI.Paginators;
using TestHelper.UI.Strategies;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TestHelper.UI
{
    /// <summary>
    /// Find <c>GameObject</c> by name or path (glob). Wait until they appear.
    /// </summary>
    public class GameObjectFinder
    {
        private static Scene s_dontDestroyOnLoadScene;

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

        private static Scene GetDontDestroyOnLoadScene()
        {
            if (s_dontDestroyOnLoadScene.IsValid())
            {
                return s_dontDestroyOnLoadScene;
            }

            var gameObject = new GameObject("DontDestroyOnLoad Object, Created by GameObjectFinder");
            Object.DontDestroyOnLoad(gameObject);
            s_dontDestroyOnLoadScene = gameObject.scene;

            return s_dontDestroyOnLoadScene;
        }

        private static List<Scene> GetAllScenes()
        {
            var scenes = new List<Scene> { GetDontDestroyOnLoadScene() };
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    scenes.Add(scene);
                }
            }

            return scenes;
        }

        private static IEnumerable<GameObject> FindGameObjectRecursive(GameObject current, IGameObjectMatcher matcher)
        {
            if (!current.activeInHierarchy)
            {
                yield break;
            }

            if (matcher.IsMatch(current))
            {
                yield return current;
            }

            foreach (Transform childTransform in current.transform)
            {
                foreach (var found in FindGameObjectRecursive(childTransform.gameObject, matcher))
                {
                    yield return found;
                }
            }
        }

        private enum Reason
        {
            NotFound,
            NotReachable,
            NotInteractable,
            MultipleMatching,
            None
        }

        private bool FilterToOnlyReachable(ref List<GameObject> objects)
        {
            for (var i = objects.Count - 1; i >= 0; i--)
            {
                var current = objects[i];
                if (_reachableStrategy.IsReachable(current, out var raycastResult))
                {
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

        private static List<GameObject> FindAllByMatcher(IGameObjectMatcher matcher, Scene scene)
        {
            var scenes = scene != default ? new List<Scene> { scene } : GetAllScenes();
            var foundObjects = new List<GameObject>();
            var rootGameObjects = new List<GameObject>();
            foreach (var loadedScene in scenes)
            {
                // Do not rely on GetRootGameObjects clearing the buffer. It does not on Unity 6000.5,
                // so a reused buffer keeps the previous scenes' roots and walks them again. With three
                // or more scenes loaded, that makes a single GameObject match more than once.
                rootGameObjects.Clear();
                loadedScene.GetRootGameObjects(rootGameObjects);
                foreach (var rootGameObject in rootGameObjects)
                {
                    foreach (var found in FindGameObjectRecursive(rootGameObject, matcher))
                    {
                        foundObjects.Add(found);
                    }
                }
            }

            return foundObjects;
        }

        private (GameObject, RaycastResult, Reason) FindByMatcher(IGameObjectMatcher matcher,
            bool reachable, bool interactable, Scene scene = default)
        {
            var foundObjects = FindAllByMatcher(matcher, scene);
            if (foundObjects.Count == 0)
            {
                return (null, default, Reason.NotFound);
            }

            if (reachable && !FilterToOnlyReachable(ref foundObjects))
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

            var resultObject = foundObjects[0];
            if (!reachable)
            {
                return (resultObject, new RaycastResult(), Reason.None);
            }

            _reachableStrategy.IsReachable(resultObject, out var raycastResult);
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
                var (foundObject, raycastResult, reason) = FindByMatcher(matcher, reachable, interactable);

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
