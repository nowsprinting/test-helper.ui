// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.UI.Paginators;
using UnityEngine;
using UnityEngine.Scripting;

namespace TestHelper.UI.TestDoubles
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    // Keeping only one public constructor is rejected: this fake verifies that PaginatorPool
    // rejects renting an IPaginator type with multiple public constructors.
    public class FakePaginatorWithMultiplePublicConstructors : IPaginator<FakeComponent>
    {
        public int IntValue { get; }
        public MonoBehaviour TargetComponent { get; set; }

        [Preserve]
        public FakePaginatorWithMultiplePublicConstructors() { }

        [Preserve]
        public FakePaginatorWithMultiplePublicConstructors(int intValue)
        {
            IntValue = intValue;
        }

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
