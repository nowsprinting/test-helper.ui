// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI.TestDoubles
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    // Intentionally has no public constructor: this fake verifies that PaginatorPool rejects
    // renting an IPaginator type without one, so it must stay non-instantiable directly.
    [SuppressMessage("ReSharper", "ClassCannotBeInstantiated")]
#pragma warning disable TestHelperUI4006
    public class FakePaginatorWithoutPublicConstructor : IPaginator<FakeComponent>
#pragma warning restore TestHelperUI4006
    {
        public MonoBehaviour TargetComponent { get; set; }

        /// <summary>
        /// non-public constructor
        /// </summary>
        private FakePaginatorWithoutPublicConstructor() { }

        public UniTask ResetAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public bool HasNextPage()
        {
            throw new NotImplementedException();
        }
    }
}
