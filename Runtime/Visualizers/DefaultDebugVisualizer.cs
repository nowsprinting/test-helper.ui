// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TestHelper.UI.Strategies;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TestHelper.UI.Visualizers
{
    /// <summary>
    /// Implementation of visualizers for debugging that use default pictograms.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class DefaultDebugVisualizer : IVisualizer, IDisposable
    {
        private const string ResourcesBasePath = "com.nowsprinting.test-helper.ui";

        /// <summary>
        /// Image file path with the "not reachable" state in the Resources folder.
        /// </summary>
        public string NotReachablePictPath { private get; set; } = $"{ResourcesBasePath}/eye_slash";

        /// <summary>
        /// Image color for the "not reachable" state.
        /// </summary>
        public Color NotReachablePictColor { private get; set; } = new Color(1f, 0f, 0f);

        /// <summary>
        /// Indicate color for the blocker.
        /// </summary>
        public Color NotReachableBlockerColor { private get; set; } = new Color(1f, 1f, 1f);

        /// <summary>
        /// Image file path with the "not interactable" state in the Resources folder.
        /// </summary>
        public string NotInteractablePictPath { private get; set; } = $"{ResourcesBasePath}/hand_slash";

        /// <summary>
        /// Image color for the "not interactable" state.
        /// </summary>
        public Color NotInteractablePictColor { private get; set; } = new Color(1f, 0f, 0f);

        /// <summary>
        /// Image file path with the "ignored" state in the Resources folder.
        /// </summary>
        public string IgnoredPictPath { private get; set; } = $"{ResourcesBasePath}/lock";

        /// <summary>
        /// Image color for the "ignored" state.
        /// </summary>
        public Color IgnoredPictColor { private get; set; } = new Color(1f, 0.68f, 0f);

        /// <summary>
        /// Screen resolution (short side) for which the pictograms size is intended.
        /// </summary>
        public int ReferenceScreenResolutionShortSide { private get; set; } = 480;

        /// <summary>
        /// Overlay <c>Canvas</c> sorting order.
        /// This can only be specified before the first pictogram is shown.
        /// </summary>
        public int CanvasSortingOrder { private get; set; } = 1000;

        /// <summary>
        /// Indicator lifetime in seconds.
        /// </summary>
        public float IndicatorLifetime { private get; set; } = 1.0f;

        /// <summary>
        /// Function of get screen point.
        /// </summary>
        public Func<GameObject, Vector2> GetScreenPoint { private get; set; } =
            DefaultScreenPointStrategy.GetScreenPoint;

        private readonly Dictionary<string, Sprite> _pics = new Dictionary<string, Sprite>();
        private readonly Stack<GameObject> _blockerIndicatorPool = new Stack<GameObject>();
        private readonly Stack<GameObject> _indicatorPool = new Stack<GameObject>();
        private Canvas _overlayCanvas;

        public void Dispose()
        {
            if (_overlayCanvas)
            {
                Object.Destroy(_overlayCanvas);
            }
        }

        /// <inheritdoc/>
        public void ShowNotReachableIndicator(Vector2 screenPoint, GameObject blocker = null)
        {
            try
            {
                if (blocker && blocker.TryGetComponent<RectTransform>(out var blockerRectTransform))
                {
                    var blockerIndicator = GetOrCreateBlockerIndicator(blockerRectTransform);
                    blockerIndicator.transform.position = screenPoint;
                }
                // TODO: 3D objects

                var indicator = GetOrCreateIndicator(NotReachablePictPath, NotReachablePictColor);
                indicator.transform.position = screenPoint;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to show not reachable indicator: {e}");
            }
        }

        /// <inheritdoc/>
        public void ShowNotInteractableIndicator(GameObject gameObject)
        {
            try
            {
                var indicator = GetOrCreateIndicator(NotInteractablePictPath, NotInteractablePictColor);
                indicator.transform.position = GetScreenPoint(gameObject);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to show not interactable indicator: {e}");
            }
        }

        /// <inheritdoc/>
        public void ShowIgnoredIndicator(GameObject gameObject)
        {
            try
            {
                var indicator = GetOrCreateIndicator(IgnoredPictPath, IgnoredPictColor);
                indicator.transform.position = GetScreenPoint(gameObject);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to show ignored indicator: {e}");
            }
        }

        private GameObject GetOrCreateBlockerIndicator(RectTransform blockerRectTransform)
        {
            GameObject indicator;
            if (_blockerIndicatorPool.Count > 0)
            {
                indicator = _blockerIndicatorPool.Pop();
                indicator.SetActive(true);
            }
            else
            {
                indicator = new GameObject($"Blocker Indicator", typeof(Image),
                    typeof(FadeOutBehaviour));
                indicator.transform.SetParent(GetOrCreateOverlayCanvas().transform);

                var image = indicator.GetComponent<Image>();
                image.raycastTarget = false; // Disable raycast target to avoid blocking UI interactions
                image.color = NotReachableBlockerColor;

                var fadeout = indicator.GetComponent<FadeOutBehaviour>();
                fadeout.Lifetime = IndicatorLifetime;
                fadeout.Acceleration = 0.2f; // Decelerated fade-out
                fadeout.OnFadeOutCompleted = () =>
                {
                    indicator.SetActive(false);
                    _blockerIndicatorPool.Push(indicator);
                };
            }

            var rectTransform = indicator.GetComponent<RectTransform>();
            rectTransform.sizeDelta = blockerRectTransform.rect.size * blockerRectTransform.lossyScale;
            rectTransform.position = blockerRectTransform.position;

            return indicator;
        }

        private GameObject GetOrCreateIndicator(string pictPath, Color pictColor)
        {
            GameObject indicator;
            if (_indicatorPool.Count > 0)
            {
                indicator = _indicatorPool.Pop();
                indicator.SetActive(true);
            }
            else
            {
                indicator = new GameObject($"Indicator", typeof(Image), typeof(ContentSizeFitter),
                    typeof(FadeOutBehaviour));
                indicator.transform.SetParent(GetOrCreateOverlayCanvas().transform);

                var contentSizeFitter = indicator.GetComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var fadeout = indicator.GetComponent<FadeOutBehaviour>();
                fadeout.Lifetime = IndicatorLifetime;
                fadeout.Acceleration = 2.0f; // Accelerated fade-out
                fadeout.OnFadeOutCompleted = () =>
                {
                    indicator.SetActive(false);
                    _indicatorPool.Push(indicator);
                };
            }

            indicator.transform.localScale = CalcScale();
            // Note: Why not use CanvasScaler? Screen points may move depending on the aspect ratio.

            var image = indicator.GetComponent<Image>();
            image.raycastTarget = false; // Disable raycast target to avoid blocking UI interactions
            image.sprite = GetOrCreateSprite(pictPath);
            image.color = pictColor;

            return indicator;
        }

        private Vector3 CalcScale()
        {
            var shortSide = Math.Min(Screen.width, Screen.height);
            var scale = (float)shortSide / ReferenceScreenResolutionShortSide;
            return new Vector3(scale, scale, 1f);
        }

        private Sprite GetOrCreateSprite(string pictPath)
        {
            if (_pics.TryGetValue(pictPath, out var sprite) && sprite != null)
            {
                return sprite;
            }

            sprite = Resources.Load<Sprite>(pictPath);
            if (sprite == null)
            {
                throw new InvalidOperationException($"Sprite not found at path: {pictPath}");
            }

            _pics[pictPath] = sprite;
            return sprite;
        }

        private Canvas GetOrCreateOverlayCanvas()
        {
            if (_overlayCanvas)
            {
                return _overlayCanvas;
            }

            var gameObject = new GameObject("DebugVisualizer Overlay Canvas", typeof(Canvas));
            Object.DontDestroyOnLoad(gameObject);

            _overlayCanvas = gameObject.GetComponent<Canvas>();
            _overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _overlayCanvas.sortingOrder = CanvasSortingOrder;

            return _overlayCanvas;
        }
    }
}
