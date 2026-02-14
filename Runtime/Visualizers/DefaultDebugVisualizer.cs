// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
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
        /// Image file path of the "not reachable" state in the Resources folder.
        /// </summary>
        public string NotReachablePictPath { private get; set; } = $"{ResourcesBasePath}/eye_slash";

        /// <summary>
        /// Image color of the "not reachable" state.
        /// </summary>
        public Color NotReachablePictColor { private get; set; } = new Color(1f, 0f, 0f);

        /// <summary>
        /// Indicate color of the blocker.
        /// </summary>
        public Color NotReachableBlockerColor { private get; set; } = new Color(1f, 1f, 1f);

        /// <summary>
        /// Image file path of the "not interactable" state in the Resources folder.
        /// </summary>
        public string NotInteractablePictPath { private get; set; } = $"{ResourcesBasePath}/hand_slash";

        /// <summary>
        /// Image color of the "not interactable" state.
        /// </summary>
        public Color NotInteractablePictColor { private get; set; } = new Color(1f, 0f, 0f);

        /// <summary>
        /// Image file path of the "ignored" state in the Resources folder.
        /// </summary>
        public string IgnoredPictPath { private get; set; } = $"{ResourcesBasePath}/lock";

        /// <summary>
        /// Image color of the "ignored" state.
        /// </summary>
        public Color IgnoredPictColor { private get; set; } = new Color(1f, 0.68f, 0f);

        /// <summary>
        /// Image file path of the pointer operation in the Resources folder.
        /// </summary>
        public string RipplePictPath { private get; set; } = $"{ResourcesBasePath}/ripple";

        /// <summary>
        /// Image color of the pointer operation.
        /// </summary>
        public Color RipplePictColor { private get; set; } = new Color(0.2f, 0.82f, 1f);

        /// <summary>
        /// Ripple scale amount per second.
        /// </summary>
        public float RippleScalePerSecond { private get; set; } = 7.0f;

        /// <summary>
        /// Number of ripples.
        /// </summary>
        public int RippleCount { private get; set; } = 3;

        /// <summary>
        /// Interval of ripples in milliseconds.
        /// </summary>
        public int RippleIntervalMillis { private get; set; } = 400;

        /// <summary>
        /// Screen resolution (short side) for which the pictograms size is intended.
        /// </summary>
        public int ReferenceScreenResolutionShortSide { private get; set; } = 480;

        /// <summary>
        /// Overlay <see cref="Canvas"/> sorting order.
        /// This can only be specified before the first pictogram is shown.
        /// </summary>
        public short CanvasSortingOrder { private get; set; } = 1000;

        /// <summary>
        /// Indicator lifetime in seconds.
        /// </summary>
        public float IndicatorLifetime { internal get; set; } = 1.0f;

        /// <summary>
        /// Function of get screen point.
        /// </summary>
        public Func<GameObject, Vector2> GetScreenPoint { get; set; } = DefaultScreenPointStrategy.GetScreenPoint;

        private readonly Dictionary<string, Sprite> _pics = new Dictionary<string, Sprite>();
        private readonly Stack<GameObject> _blockerIndicatorPool = new Stack<GameObject>();
        private readonly Stack<GameObject> _indicatorPool = new Stack<GameObject>();
        private readonly Stack<GameObject> _ripplePool = new Stack<GameObject>();
        private Canvas _overlayCanvas;

        /// <inheritdoc/>
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
                    PopOrCreateBlockerIndicator(blockerRectTransform);
                }
                // TODO: 3D objects

                PopOrCreateIndicator(screenPoint, NotReachablePictPath, NotReachablePictColor);
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
                PopOrCreateIndicator(GetScreenPoint(gameObject), NotInteractablePictPath, NotInteractablePictColor);
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
                PopOrCreateIndicator(GetScreenPoint(gameObject), IgnoredPictPath, IgnoredPictColor);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to show ignored indicator: {e}");
            }
        }

        /// <inheritdoc/>
        public void ShowPointerOperationEffect(GameObject gameObject)
        {
            ShowPointerOperationEffect(GetScreenPoint(gameObject));
        }

        /// <inheritdoc/>
        public void ShowPointerOperationEffect(Vector2 screenPoint)
        {
            try
            {
                for (var i = 0; i < RippleCount; i++)
                {
                    ShowRippleEffectAfterDelay(i).Forget();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to show ripple effect: {e}");
            }

            return;

            async UniTask ShowRippleEffectAfterDelay(int i)
            {
                var interval = RippleIntervalMillis * i;
                if (interval > 0)
                {
                    await UniTask.Delay(interval, ignoreTimeScale: Time.timeScale == 0f);
                }

                var elapsedMillis = Math.Max(interval - RippleIntervalMillis * 0.5f, 0);
                PopOrCreateRipple(screenPoint, elapsedMillis * 0.001f);
            }
        }

        private void PopOrCreateBlockerIndicator(RectTransform blockerRectTransform)
        {
            GameObject indicator;
            if (_blockerIndicatorPool.Count > 0)
            {
                indicator = _blockerIndicatorPool.Pop();
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

            indicator.SetActive(true);
        }

        private void PopOrCreateIndicator(Vector2 screenPoint, string pictPath, Color pictColor)
        {
            GameObject indicator;
            if (_indicatorPool.Count > 0)
            {
                indicator = _indicatorPool.Pop();
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

            var scale = CalcScale();
            indicator.transform.localScale = new Vector3(scale, scale, 1f);
            // Note: Why not use CanvasScaler? Screen points may move depending on the aspect ratio.

            var image = indicator.GetComponent<Image>();
            image.raycastTarget = false; // Disable raycast target to avoid blocking UI interactions
            image.sprite = GetOrCreateSprite(pictPath);
            image.color = pictColor;

            indicator.transform.position = screenPoint;
            indicator.SetActive(true);
        }

        private void PopOrCreateRipple(Vector2 screenPoint, float elapsed)
        {
            GameObject ripple;
            if (_ripplePool.Count > 0)
            {
                ripple = _ripplePool.Pop();
            }
            else
            {
                ripple = new GameObject($"Ripple", typeof(Image), typeof(ContentSizeFitter),
                    typeof(FadeOutBehaviour), typeof(SpreadBehaviour));
                ripple.transform.SetParent(GetOrCreateOverlayCanvas().transform);

                var contentSizeFitter = ripple.GetComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                var image = ripple.GetComponent<Image>();
                image.raycastTarget = false; // Disable raycast target to avoid blocking UI interactions
                image.sprite = GetOrCreateSprite(RipplePictPath);
                image.color = RipplePictColor;

                var fadeout = ripple.GetComponent<FadeOutBehaviour>();
                fadeout.Lifetime = IndicatorLifetime;
                fadeout.Acceleration = 2.0f; // Accelerated fade-out
                fadeout.OnFadeOutCompleted = () =>
                {
                    ripple.SetActive(false);
                    _ripplePool.Push(ripple);
                };
            }

            var scale = CalcScale();
            ripple.transform.localScale = new Vector3(scale, scale, 1f);
            // Note: Why not use CanvasScaler? Screen points may move depending on the aspect ratio.

            var spreadBehaviour = ripple.GetComponent<SpreadBehaviour>();
            spreadBehaviour.ScalePerSecond = RippleScalePerSecond * scale;

            ripple.GetComponent<FadeOutBehaviour>().InitialElapsed = elapsed;

            ripple.transform.position = screenPoint;
            ripple.SetActive(true);
        }

        private float CalcScale()
        {
            var shortSide = Math.Min(Screen.width, Screen.height);
            return (float)shortSide / ReferenceScreenResolutionShortSide;
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
