// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
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
        private readonly Dictionary<Type, Stack<IPaginator>> _pools = new Dictionary<Type, Stack<IPaginator>>();
        private readonly Dictionary<Type, object[]> _registrations = new Dictionary<Type, object[]>();
        private readonly bool _requireRegistration;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatorPool"/> class.
        /// </summary>
        /// <param name="requireRegistration">If true, only paginators registered via <c>Register&lt;T&gt;</c> can be rented.</param>
        public PaginatorPool(bool requireRegistration = true)
        {
            _requireRegistration = requireRegistration;
        }

        /// <summary>
        /// Registers a paginator type with its constructor arguments.
        /// </summary>
        /// <remarks>
        /// Instances are created by invoking the public constructor of <typeparamref name="T"/> via reflection.
        /// Annotate every public constructor of <typeparamref name="T"/> with <c>[UnityEngine.Scripting.Preserve]</c> so that managed code stripping does not remove it from the Player build.
        /// <p/>
        /// When <paramref name="args"/> is omitted, <typeparamref name="T"/> must have exactly one public constructor, and all its parameters must have default values.
        /// If <typeparamref name="T"/> has multiple public constructors, specify <paramref name="args"/> to select one; otherwise, <c>Rent</c> throws <see cref="InvalidOperationException"/>.
        /// <p/>
        /// Do not include the target component in <paramref name="args"/>; <c>Rent</c> assigns it via <see cref="IPaginator.TargetComponent"/>.
        /// </remarks>
        /// <typeparam name="T">The paginator type to register</typeparam>
        /// <param name="args">Constructor arguments for creating instances</param>
        public PaginatorPool Register<T>(params object[] args) where T : class, IPaginator
        {
            args = args ?? Array.Empty<object>();
            _registrations[typeof(T)] = args;
            return this;
        }

        /// <summary>
        /// Rents a paginator instance from the pool or creates a new one, and assigns the target component to it.
        /// </summary>
        /// <typeparam name="T">The paginator type to rent</typeparam>
        /// <param name="targetComponent">The pageable component to be controlled by the paginator. If omitted, the paginator has no target component.</param>
        /// <returns>An instance of the requested paginator type</returns>
        public T Rent<T>(MonoBehaviour targetComponent = null) where T : class, IPaginator
        {
            return (T)Rent(typeof(T), targetComponent);
        }

        /// <summary>
        /// Rents a paginator instance from the pool or creates a new one, and assigns the target component to it.
        /// </summary>
        /// <param name="type">The paginator type to rent</param>
        /// <param name="targetComponent">The pageable component to be controlled by the paginator. If omitted, the paginator has no target component.</param>
        /// <returns>An instance of the requested paginator type</returns>
        /// <exception cref="ArgumentException">When the paginator does not support <paramref name="targetComponent"/></exception>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public IPaginator Rent(Type type, MonoBehaviour targetComponent = null)
        {
            var paginator = RentWithoutTargetComponent(type);
            paginator.TargetComponent = targetComponent;
            return paginator;
        }

        private IPaginator RentWithoutTargetComponent(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!typeof(IPaginator).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{type.Name} does not implement IPaginator.");
            }

            if (_pools.TryGetValue(type, out var stack) && stack.Count > 0)
            {
                return stack.Pop();
            }

            if (!_registrations.TryGetValue(type, out var args) && _requireRegistration)
            {
                throw new InvalidOperationException($"{type.Name} is not registered.");
            }

            if (args?.Length > 0)
            {
                return (IPaginator)Activator.CreateInstance(type, args);
            }

            var constructors = type.GetConstructors();
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException($"{type.Name} has no public constructor.");
            }

            // Picking one of multiple constructors (e.g., the first, or the one with the most parameters) is rejected:
            // GetConstructors() does not guarantee the order, and managed code stripping on the Player can remove
            // some of them, so the chosen constructor could silently differ between the Editor and the Player.
            if (constructors.Length > 1)
            {
                throw new InvalidOperationException(
                    $"{type.Name} has multiple public constructors. Register with explicit constructor arguments.");
            }

            var parameters = constructors[0].GetParameters();
            var defaultArgs = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                if (!parameters[i].HasDefaultValue)
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve required parameter '{parameters[i].Name}' of type {parameters[i].ParameterType.Name}. " +
                        "Register with explicit constructor arguments or add a default value.");
                }

                defaultArgs[i] = parameters[i].DefaultValue;
            }

            return (IPaginator)constructors[0].Invoke(defaultArgs);
        }

        /// <summary>
        /// Returns a paginator instance to the pool for reuse.
        /// Clears its target component so that the pool does not keep the component alive.
        /// </summary>
        /// <param name="obj">The paginator instance to return</param>
        public void Return(IPaginator obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var type = obj.GetType();
            if (!_registrations.ContainsKey(type) && _requireRegistration)
            {
                throw new InvalidOperationException($"{type.Name} is not registered.");
            }

            obj.TargetComponent = null;

            if (!_pools.TryGetValue(type, out var stack))
            {
                stack = new Stack<IPaginator>();
                _pools[type] = stack;
            }

            stack.Push(obj);
        }
    }
}
