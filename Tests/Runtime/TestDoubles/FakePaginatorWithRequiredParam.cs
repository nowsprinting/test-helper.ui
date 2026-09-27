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
    public class FakePaginatorWithRequiredParam : IPaginator<FakeComponent>
    {
        public string RequiredParam { get; }
        public MonoBehaviour TargetComponent { get; set; }

        // A default value is rejected: this fake verifies that PaginatorPool rejects renting an
        // IPaginator type whose constructor parameter has no default value.
        [Preserve]
#pragma warning disable TestHelperUI4007
        public FakePaginatorWithRequiredParam(string requiredParam)
#pragma warning restore TestHelperUI4007
        {
            RequiredParam = requiredParam;
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
