// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TestHelper.UI.Paginators
{
    /// <summary>
    /// Interface for pagination controller for finding <c>GameObject</c> on pageable or scrollable UI components (e.g., <c>ScrollRect</c>, Carousel, Paged dialog).
    /// Provides intuitive pagination operations as page navigation functionality, enabling auxiliary operations in the <see cref="GameObjectFinder"/>.
    /// </summary>
    /// <remarks>
    /// The implementation class must meet the following requirements:
    /// <list type="bullet">
    ///     <item><c>PaginatorPool</c> creates instances by invoking the public constructor via reflection.
    ///     Annotate every public constructor with <c>[UnityEngine.Scripting.Preserve]</c> so that managed code
    ///     stripping does not remove it from the Player build</item>
    ///     <item>A paginator must have exactly one public constructor whose parameters all have default values;
    ///     otherwise, register it with explicit constructor arguments via <c>PaginatorPool.Register&lt;T&gt;(args)</c></item>
    ///     <item>The <see cref="TargetComponent"/> setter must accept null, reset the internal state tied to the
    ///     previous target component (pooled instances are reassigned), and throw <see cref="System.ArgumentException"/>
    ///     for an unsupported component type</item>
    ///     <item><see cref="ResetAsync"/> and <see cref="HasNextPage"/> must throw
    ///     <see cref="System.InvalidOperationException"/> when the target component is not set</item>
    ///     <item><see cref="NextPageAsync"/> must eventually return false: callers loop on the return value, so
    ///     when the page position cannot advance (e.g., the layout has not been calculated yet), it must return
    ///     false instead of true</item>
    /// </list>
    /// <seealso cref="TestHelper.UI.Paginators.IPaginatorTest"/>
    /// </remarks>
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public interface IPaginator
    {
        /// <summary>
        /// The pageable component to be controlled (scrollable components are a kind of pageable component).
        /// Set null to clear it.
        /// </summary>
        /// <exception cref="System.ArgumentException">When the value is not the component type this paginator supports, or is in an invalid state</exception>
        MonoBehaviour TargetComponent { set; }

        /// <summary>
        /// Move the page position to the beginning.
        /// For scroll components, the display position (top, bottom, left, or right) depends on the implementation.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        UniTask ResetAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Move to the next page.
        /// For scroll components, advance the page by the size of the display area.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if page navigation was executed, false if not executed because the end has been reached or the page position cannot advance</returns>
        UniTask<bool> NextPageAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get whether the next page exists.
        /// </summary>
        /// <returns>True if the next page exists, false if the end has been reached</returns>
        bool HasNextPage();
    }
}
