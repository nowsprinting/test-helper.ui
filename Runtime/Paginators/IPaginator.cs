// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.UI.Paginators
{
    /// <summary>
    /// Pagination controller for finding <c>GameObject</c> on pageable UI components (e.g., <c>ScrollRect</c>, Carousel, Paged dialog).
    /// Provides page navigation as an auxiliary operation for <see cref="GameObjectFinder"/>.
    /// </summary>
    /// <remarks>
    /// Implement this interface for a paginator; <typeparamref name="TComponent"/> declares the pageable component type it controls,
    /// and <c>PaginatorPool.Rent(MonoBehaviour)</c> selects the paginator whose <typeparamref name="TComponent"/> exactly matches the target component type.
    /// <p/>
    /// The implementation class must meet the following requirements:
    /// <list type="bullet">
    ///     <item><c>PaginatorPool</c> creates instances by invoking the public constructor via reflection.
    ///     Annotate every public constructor with <c>[UnityEngine.Scripting.Preserve]</c> so that managed code
    ///     stripping does not remove it from the Player build</item>
    ///     <item>Have exactly one public constructor whose parameters all have default values;
    ///     otherwise, register it with explicit constructor arguments via <c>PaginatorPool.Register&lt;T&gt;(args)</c></item>
    ///     <item>Implement the members of <see cref="IPaginator"/> as documented on each member</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="TComponent">The type of the pageable component to be controlled</typeparam>
    // Covariance/contravariance (in/out) is rejected: it would make a paginator for a base component type match a
    // derived component type, whereas PaginatorPool selects by exact type match.
    public interface IPaginator<TComponent> : IPaginator where TComponent : MonoBehaviour
    {
    }

    /// <summary>
    /// Non-generic base of <see cref="IPaginator{TComponent}"/>, used where the component type is not statically known
    /// (e.g., <see cref="GameObjectFinder"/>, <c>PaginatorPool</c>).
    /// </summary>
    /// <remarks>
    /// Implement <see cref="IPaginator{TComponent}"/> rather than this interface directly;
    /// a paginator implementing only this interface cannot be selected by <c>PaginatorPool.Rent(MonoBehaviour)</c>.
    /// </remarks>
    public interface IPaginator
    {
        /// <summary>
        /// The pageable component to be controlled (scrollable components are a kind of pageable component).
        /// Set null to clear it.
        /// </summary>
        /// <remarks>
        /// The setter must reset the internal state tied to the previous target component, since <c>PaginatorPool</c> reassigns it to pooled instances.
        /// </remarks>
        /// <exception cref="System.ArgumentException">When the value is not the component type this paginator supports, or is in an invalid state</exception>
        MonoBehaviour TargetComponent { set; }

        /// <summary>
        /// Move the page position to the beginning.
        /// For scroll components, the display position (top, bottom, left, or right) depends on the implementation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="System.InvalidOperationException">When the target component is not set</exception>
        UniTask ResetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Move to the next page.
        /// For scroll components, advance the page by the size of the display area.
        /// </summary>
        /// <remarks>
        /// Must eventually return false: callers (e.g., <see cref="GameObjectFinder"/>) loop on the return value,
        /// so when the page position cannot advance (e.g., the layout has not been calculated yet), return false instead of true.
        /// </remarks>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if page navigation was executed, false if not executed because the end has been reached or the page position cannot advance</returns>
        /// <exception cref="System.InvalidOperationException">When the target component is not set</exception>
        UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get whether the next page exists.
        /// </summary>
        /// <returns>True if the next page exists, false if the end has been reached</returns>
        /// <exception cref="System.InvalidOperationException">When the target component is not set</exception>
        bool HasNextPage();
    }
}
