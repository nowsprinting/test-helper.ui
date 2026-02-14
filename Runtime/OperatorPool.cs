// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TestHelper.UI.Operators;

namespace TestHelper.UI
{
    /// <summary>
    /// Manages object pooling for IOperator implementations.
    /// </summary>
    public class OperatorPool
    {
        private readonly Dictionary<Type, Stack<IOperator>> _pools = new Dictionary<Type, Stack<IOperator>>();
        private readonly Dictionary<Type, object[]> _registrations = new Dictionary<Type, object[]>();

        /// <summary>
        /// Registers an operator type with its constructor arguments.
        /// </summary>
        /// <typeparam name="T">The operator type to register</typeparam>
        /// <param name="args">Constructor arguments for creating instances</param>
        public void Register<T>(params object[] args) where T : class, IOperator
        {
            _registrations[typeof(T)] = args;
        }

        /// <summary>
        /// Rents an operator instance from the pool or creates a new one.
        /// </summary>
        /// <typeparam name="T">The operator type to rent</typeparam>
        /// <returns>An instance of the requested operator type</returns>
        public T Rent<T>() where T : class, IOperator
        {
            var type = typeof(T);

            if (_pools.TryGetValue(type, out var stack) && stack.Count > 0)
            {
                return (T)stack.Pop();
            }

            if (!_registrations.TryGetValue(type, out var args))
            {
                throw new InvalidOperationException($"Type {type.Name} is not registered.");
            }

            if (args.Length > 0)
            {
                return (T)Activator.CreateInstance(type, args);
            }

            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null)
            {
                throw new InvalidOperationException($"Type {type.Name} has no public constructor.");
            }

            var parameters = constructor.GetParameters();
            var defaultArgs = parameters.Select(p => p.DefaultValue).ToArray();
            return (T)constructor.Invoke(defaultArgs);
        }

        /// <summary>
        /// Returns an operator instance to the pool for reuse.
        /// </summary>
        /// <param name="obj">The operator instance to return</param>
        public void Return(IOperator obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var type = obj.GetType();
            if (!_registrations.ContainsKey(type))
            {
                throw new InvalidOperationException($"Type {type.Name} is not registered.");
            }

            if (!_pools.TryGetValue(type, out var stack))
            {
                stack = new Stack<IOperator>();
                _pools[type] = stack;
            }

            stack.Push(obj);
        }
    }
}
