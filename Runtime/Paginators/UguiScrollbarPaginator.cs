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
    /// Paginator implementation for <see cref="Scrollbar"/>.
    /// </summary>
    public class UguiScrollbarPaginator : IPaginator
    {
        private Scrollbar _scrollbar;

        /// <summary>
        /// Constructor that takes a scroller instance.
        /// </summary>
        /// <param name="scrollbar">Scrollbar to be controlled. If omitted, assign it via <see cref="TargetComponent"/> later.</param>
        [Preserve]
        public UguiScrollbarPaginator(Scrollbar scrollbar = null)
        {
            TargetComponent = scrollbar;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException">When the value is not a <see cref="Scrollbar"/></exception>
        public MonoBehaviour TargetComponent
        {
            set
            {
                var scrollbar = value as Scrollbar;
                if (value && !scrollbar)
                {
                    throw new ArgumentException($"TargetComponent must be a Scrollbar, but was {value.GetType().Name}.",
                        nameof(value));
                }

                _scrollbar = scrollbar;
            }
        }

        /// <inheritdoc />
        public async UniTask ResetAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfTargetComponentNotSet();
            _scrollbar.value = 0f;
            await UniTask.Yield(cancellationToken);
        }

        /// <inheritdoc />
        public async UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default)
        {
            if (!HasNextPage())
            {
                return false;
            }

            var currentValue = _scrollbar.value;
            var scrollAmount = CalculateNormalizedScrollAmount();
            var newValue = MathF.Min(currentValue + scrollAmount, 1f);

            _scrollbar.value = newValue;
            await UniTask.Yield(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public bool HasNextPage()
        {
            ThrowIfTargetComponentNotSet();

            // A zero scroll amount (Scrollbar.size is zero before Unity's layout calculation) is also "no next
            // page"; judging by value alone would let NextPageAsync return true forever because the value can
            // never advance.
            return CalculateNormalizedScrollAmount() > 0f && _scrollbar.value < 1.0f - float.Epsilon;
        }

        private void ThrowIfTargetComponentNotSet()
        {
            if (!_scrollbar)
            {
                throw new InvalidOperationException("Target component is not set.");
            }
        }

        private float CalculateNormalizedScrollAmount()
        {
            // Use the size property of Scrollbar (represents the ratio of the display area)
            return _scrollbar.size;
        }
    }
}
