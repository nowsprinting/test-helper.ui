// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using TestHelper.Random;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using TestHelper.UI.Visualizers;
using UnityEngine;

namespace TestHelper.UI
{
    /// <summary>
    /// Manages object pooling for IOperator implementations.
    /// </summary>
    public class OperatorPool
    {
        private readonly Dictionary<Type, Stack<IOperator>> _pools = new Dictionary<Type, Stack<IOperator>>();
        private readonly Dictionary<Type, object[]> _registrations = new Dictionary<Type, object[]>();

        private readonly ILogger _logger;
        private readonly ScreenshotOptions _screenshotOptions;
        private readonly IVisualizer _visualizer;
        private readonly Func<GameObject, Vector2> _getScreenPoint;
        private readonly IReachableStrategy _reachableStrategy;
        private readonly IRandom _random;

        /// <summary>
        /// Initializes a new instance of the <see cref="OperatorPool"/> class.
        /// </summary>
        /// <param name="logger">Logger to inject into operators</param>
        /// <param name="screenshotOptions">Screenshot options to inject into operators</param>
        /// <param name="visualizer">Visualizer to inject into operators</param>
        /// <param name="getScreenPoint">Screen point function to inject into operators</param>
        /// <param name="reachableStrategy">Reachable strategy to inject into operators</param>
        /// <param name="random">The parent of the random instance to inject into the operators</param>
        public OperatorPool(
            ILogger logger = null,
            ScreenshotOptions screenshotOptions = null,
            IVisualizer visualizer = null,
            Func<GameObject, Vector2> getScreenPoint = null,
            IReachableStrategy reachableStrategy = null,
            IRandom random = null)
        {
            _logger = logger;
            _screenshotOptions = screenshotOptions;
            _visualizer = visualizer;
            _getScreenPoint = getScreenPoint;
            _reachableStrategy = reachableStrategy;
            _random = random;
        }

        /// <summary>
        /// Registers an operator type with its constructor arguments.
        /// </summary>
        /// <typeparam name="T">The operator type to register</typeparam>
        /// <param name="args">Constructor arguments for creating instances</param>
        public OperatorPool Register<T>(params object[] args) where T : class, IOperator
        {
            args = args ?? Array.Empty<object>();
            _registrations[typeof(T)] = args;
            return this;
        }

        /// <summary>
        /// Rents all registered operator types from the pool or creates new ones.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IOperator> RentAll()
        {
            return _registrations.Keys.Select(Rent);
        }

        /// <summary>
        /// Rents an operator instance from the pool or creates a new one.
        /// </summary>
        /// <typeparam name="T">The operator type to rent</typeparam>
        /// <returns>An instance of the requested operator type</returns>
        public T Rent<T>() where T : class, IOperator
        {
            return (T)Rent(typeof(T));
        }

        /// <summary>
        /// Rents an operator instance from the pool or creates a new one.
        /// </summary>
        /// <param name="type">The operator type to rent</param>
        /// <returns>An instance of the requested operator type</returns>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public IOperator Rent(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!typeof(IOperator).IsAssignableFrom(type))
            {
                throw new InvalidOperationException($"{type.Name} does not implement IOperator.");
            }

            if (_pools.TryGetValue(type, out var stack) && stack.Count > 0)
            {
                return stack.Pop();
            }

            if (!_registrations.TryGetValue(type, out var args))
            {
                throw new InvalidOperationException($"{type.Name} is not registered.");
            }

            if (args?.Length > 0)
            {
                return (IOperator)Activator.CreateInstance(type, ConvertRegisteredArgs(args));
            }

            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null)
            {
                throw new InvalidOperationException($"{type.Name} has no public constructor.");
            }

            var parameters = constructor.GetParameters();
            var resolvedArgs = parameters.Select(ResolveArgument).ToArray();
            return (IOperator)constructor.Invoke(resolvedArgs);
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
                throw new InvalidOperationException($"{type.Name} is not registered.");
            }

            if (!_pools.TryGetValue(type, out var stack))
            {
                stack = new Stack<IOperator>();
                _pools[type] = stack;
            }

            stack.Push(obj);
        }

        private static object[] ConvertRegisteredArgs(object[] sources)
        {
            var args = new object[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                if (sources[i] is IRandom random)
                {
                    args[i] = random.Fork();
                }
                else
                {
                    args[i] = sources[i];
                }
            }

            return args;
        }

        [SuppressMessage("ReSharper", "CognitiveComplexity")]
        private object ResolveArgument(ParameterInfo parameter)
        {
            var type = parameter.ParameterType;

            if (type == typeof(ILogger) && _logger != null) return _logger;
            if (type == typeof(ScreenshotOptions) && _screenshotOptions != null) return _screenshotOptions;
            if (type == typeof(IVisualizer) && _visualizer != null) return _visualizer;
            if (type == typeof(Func<GameObject, Vector2>) && _getScreenPoint != null) return _getScreenPoint;
            if (type == typeof(IReachableStrategy) && _reachableStrategy != null) return _reachableStrategy;
            if (type == typeof(IRandom) && _random != null) return _random.Fork();

            if (parameter.HasDefaultValue)
            {
                return parameter.DefaultValue;
            }

            throw new InvalidOperationException(
                $"Cannot resolve required parameter '{parameter.Name}' of type {parameter.ParameterType.Name}. " +
                $"Register with explicit constructor arguments or add a default value.");
        }
    }
}
