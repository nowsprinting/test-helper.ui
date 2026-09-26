// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using TestHelper.UI.Paginators;
using UnityEngine;

namespace TestHelper.UI
{
    /// <summary>
    /// Manages object pooling for IPaginator implementations.
    /// </summary>
    public class PaginatorPool
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatorPool"/> class.
        /// </summary>
        /// <param name="requireRegistration">If true, only paginators registered via <c>Register&lt;T&gt;</c> can be rented.</param>
        public PaginatorPool(bool requireRegistration = true)
        {
        }

        /// <summary>
        /// Registers a paginator type with its constructor arguments.
        /// </summary>
        /// <typeparam name="T">The paginator type to register</typeparam>
        /// <param name="args">Constructor arguments for creating instances</param>
        public PaginatorPool Register<T>(params object[] args) where T : class, IPaginator
        {
            return null;
        }

        /// <summary>
        /// Rents a paginator instance from the pool or creates a new one, and assigns the target component to it.
        /// </summary>
        /// <typeparam name="T">The paginator type to rent</typeparam>
        /// <param name="targetComponent">The pageable component to be controlled by the paginator</param>
        /// <returns>An instance of the requested paginator type</returns>
        public T Rent<T>(MonoBehaviour targetComponent = null) where T : class, IPaginator
        {
            return null;
        }

        /// <summary>
        /// Rents a paginator instance from the pool or creates a new one, and assigns the target component to it.
        /// </summary>
        /// <param name="type">The paginator type to rent</param>
        /// <param name="targetComponent">The pageable component to be controlled by the paginator</param>
        /// <returns>An instance of the requested paginator type</returns>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public IPaginator Rent(Type type, MonoBehaviour targetComponent = null)
        {
            return null;
        }

        /// <summary>
        /// Returns a paginator instance to the pool for reuse.
        /// </summary>
        /// <param name="obj">The paginator instance to return</param>
        public void Return(IPaginator obj)
        {
        }
    }
}
