// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private readonly Scrollbar _scrollbar;

        /// <summary>
        /// Constructor that takes a scroller instance.
        /// </summary>
        /// <param name="scrollbar">Scrollbar to be controlled</param>
        /// <exception cref="ArgumentNullException">When scrollbar is null</exception>
        public UguiScrollbarPaginator(Scrollbar scrollbar)
        {
            if (!scrollbar)
            {
                throw new ArgumentNullException(nameof(scrollbar));
            }

            _scrollbar = scrollbar;
        }

        /// <inheritdoc />
        public async UniTask ResetAsync(CancellationToken cancellationToken = default)
        {
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
            // A zero size means the scroll amount is zero (the state before Unity's layout calculation); judging by
            // value alone would let NextPageAsync return true forever because the value can never advance.
            if (_scrollbar.size <= 0f)
            {
                return false;
            }

            // For Scrollbar, if value is less than 1.0, the next page exists
            return _scrollbar.value < 1.0f - float.Epsilon;
        }

        private float CalculateNormalizedScrollAmount()
        {
            // Use the size property of Scrollbar (represents the ratio of the display area)
            return _scrollbar.size;
        }
    }
}
