// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
// System.MathF requires .NET Standard 2.1 (Unity 2021.2 or newer); aliased so that call sites need no directives.
#if UNITY_2021_2_OR_NEWER
using MathF = System.MathF;

#else
using MathF = UnityEngine.Mathf;
#endif

namespace TestHelper.UI.Paginators
{
    /// <summary>
    /// Paginator implementation for <see cref="ScrollRect"/>.
    /// </summary>
    public class UguiScrollRectPaginator : IPaginator<ScrollRect>
    {
        private ScrollRect _scrollRect;
        private bool _isHorizontalAtEnd;

        /// <summary>
        /// Constructor that takes a scroller instance.
        /// </summary>
        /// <param name="scrollRect">ScrollRect to be controlled. If omitted, assign it via <see cref="TargetComponent"/> later.</param>
        /// <exception cref="ArgumentException">When scrollRect.content is null</exception>
        [Preserve]
        public UguiScrollRectPaginator(ScrollRect scrollRect = null)
        {
            TargetComponent = scrollRect;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">When the value is not a <see cref="ScrollRect"/>, or its content is null</exception>
        public MonoBehaviour TargetComponent
        {
            set
            {
                var scrollRect = value as ScrollRect;
                if (value && !scrollRect)
                {
                    throw new ArgumentException(
                        $"TargetComponent must be a ScrollRect, but was {value.GetType().Name}.",
                        nameof(value));
                }

                if (scrollRect && !scrollRect.content)
                {
                    throw new ArgumentException("ScrollRect.content is null.", nameof(value));
                }

                _scrollRect = scrollRect;
                _isHorizontalAtEnd = false;
            }
        }

        /// <inheritdoc />
        public async UniTask ResetAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfTargetComponentNotSet();

            _scrollRect.normalizedPosition = new Vector2(0f, 1f);
            _isHorizontalAtEnd = false;
            await UniTask.Yield(cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default)
        {
            if (!HasNextPage())
            {
                return false;
            }

            var currentPosition = _scrollRect.normalizedPosition;

            if (_scrollRect.horizontal && _scrollRect.vertical)
            {
                // For both scrolling
                if (!_isHorizontalAtEnd && !IsHorizontalAtEnd())
                {
                    // Move horizontally
                    var horizontalAmount = CalculateHorizontalScrollAmount();
                    var newX = MathF.Min(currentPosition.x + horizontalAmount, 1f);
                    _scrollRect.normalizedPosition = new Vector2(newX, currentPosition.y);
                    _isHorizontalAtEnd = newX >= 1f - float.Epsilon;
                }
                else
                {
                    // Horizontal direction is at the end, so return to the left and move vertically
                    var verticalAmount = CalculateVerticalScrollAmount();
                    var newY = MathF.Max(currentPosition.y - verticalAmount, 0f);
                    _scrollRect.normalizedPosition = new Vector2(0f, newY);
                    _isHorizontalAtEnd = false;
                }
            }
            else if (_scrollRect.horizontal)
            {
                // Horizontal direction only
                var horizontalAmount = CalculateHorizontalScrollAmount();
                var newX = MathF.Min(currentPosition.x + horizontalAmount, 1f);
                _scrollRect.normalizedPosition = new Vector2(newX, currentPosition.y);
            }
            else if (_scrollRect.vertical)
            {
                // Vertical direction only
                var verticalAmount = CalculateVerticalScrollAmount();
                var newY = MathF.Max(currentPosition.y - verticalAmount, 0f);
                _scrollRect.normalizedPosition = new Vector2(currentPosition.x, newY);
            }
            else
            {
                // Scrolling disabled
                return false;
            }

            await UniTask.Yield(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public bool HasNextPage()
        {
            ThrowIfTargetComponentNotSet();

            // Dispatching by _scrollRect.horizontal/vertical is unnecessary: a disabled axis has a zero scroll
            // amount, so IsHorizontalAtEnd/IsVerticalAtEnd already report it as at the end.
            return !(IsHorizontalAtEnd() && IsVerticalAtEnd());
        }

        private void ThrowIfTargetComponentNotSet()
        {
            if (!_scrollRect)
            {
                throw new InvalidOperationException("Target component is not set.");
            }
        }

        private Vector2 CalculateViewportSize()
        {
            var viewport = _scrollRect.viewport ?? _scrollRect.transform as RectTransform;
            return viewport?.rect.size ?? Vector2.zero;
        }

        // An axis whose scroll amount is zero (viewport not laid out yet, or content fits in the viewport) is
        // treated as at the end; judging by normalizedPosition alone would let NextPageAsync return true forever
        // because the position can never advance.
        private bool IsHorizontalAtEnd()
        {
            return CalculateHorizontalScrollAmount() <= 0f
                   || _scrollRect.normalizedPosition.x >= 1.0f - float.Epsilon;
        }

        private bool IsVerticalAtEnd()
        {
            return CalculateVerticalScrollAmount() <= 0f
                   || _scrollRect.normalizedPosition.y <= 0.0f + float.Epsilon;
        }

        private float CalculateHorizontalScrollAmount()
        {
            if (!_scrollRect.horizontal)
            {
                return 0f;
            }

            var viewportSize = CalculateViewportSize();
            var contentSize = _scrollRect.content.rect.size;

            if (contentSize.x <= viewportSize.x)
            {
                return 0f; // When content is smaller than viewport
            }

            return viewportSize.x / (contentSize.x - viewportSize.x);
        }

        private float CalculateVerticalScrollAmount()
        {
            if (!_scrollRect.vertical)
            {
                return 0f;
            }

            var viewportSize = CalculateViewportSize();
            var contentSize = _scrollRect.content.rect.size;

            if (contentSize.y <= viewportSize.y)
            {
                return 0f; // When content is smaller than viewport
            }

            return viewportSize.y / (contentSize.y - viewportSize.y);
        }
    }
}
