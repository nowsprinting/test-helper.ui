// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;

namespace TestHelper.UI.Paginators
{
    /// <summary>
    /// Paginator that declares the type of the pageable component it controls.
    /// <c>PaginatorPool.Rent(MonoBehaviour)</c> selects the paginator whose <typeparamref name="TComponent"/> exactly matches the target component type.
    /// </summary>
    /// <typeparam name="TComponent">The type of the pageable component to be controlled</typeparam>
    // Covariance/contravariance (in/out) is rejected: it would make a paginator for a base component type match a
    // derived component type, whereas PaginatorPool selects by exact type match.
    public interface IPaginator<TComponent> : IPaginator where TComponent : MonoBehaviour
    {
    }
}
