// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private readonly bool _requireRegistration;

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
        /// <param name="requireRegistration">If true, only operators registered via <c>Register&lt;T&gt;</c> can be rented.</param>
        public OperatorPool(
            ILogger logger = null,
            ScreenshotOptions screenshotOptions = null,
            IVisualizer visualizer = null,
            Func<GameObject, Vector2> getScreenPoint = null,
            IReachableStrategy reachableStrategy = null,
            IRandom random = null,
            bool requireRegistration = true)
        {
            _logger = logger;
            _screenshotOptions = screenshotOptions;
            _visualizer = visualizer;
            _getScreenPoint = getScreenPoint;
            _reachableStrategy = reachableStrategy;
            _random = random;
            _requireRegistration = requireRegistration;
        }

        /// <summary>
        /// Registers an operator type with its constructor arguments.
        /// </summary>
        /// <remarks>
        /// Instances are created by invoking the public constructor of <typeparamref name="T"/> via reflection.
        /// Annotate every public constructor of <typeparamref name="T"/> with <c>[UnityEngine.Scripting.Preserve]</c> so that managed code stripping does not remove it from the Player build;
        /// for the implicit default constructor, annotate the class instead.
        /// <p/>
        /// When <paramref name="args"/> is omitted, <typeparamref name="T"/> must have exactly one public constructor; its parameters are resolved from the values injected into this pool or their default values.
        /// If <typeparamref name="T"/> has multiple public constructors, specify <paramref name="args"/> to select one; otherwise, <c>Rent</c> throws <see cref="InvalidOperationException"/>.
        /// </remarks>
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
        /// <param name="operators">List to store the rented instances. It is cleared before storing. If null, a new list is allocated</param>
        /// <returns>Instances of all registered operator types. The same instance as <paramref name="operators"/> if specified</returns>
        /// <exception cref="InvalidOperationException">When a registered operator type cannot be created without constructor arguments: it has no public constructor, has multiple public constructors, or has a parameter that is neither injected by this pool nor has a default value</exception>
        /// <exception cref="MissingMethodException">When no public constructor of a registered operator type matches the registered constructor arguments</exception>
        public IReadOnlyList<IOperator> RentAll(List<IOperator> operators = null)
        {
            if (operators == null)
            {
                operators = new List<IOperator>(_registrations.Count);
            }
            else
            {
                operators.Clear();
            }

            foreach (var type in _registrations.Keys)
            {
                operators.Add(Rent(type));
            }

            return operators;
        }

        /// <summary>
        /// Rents an operator instance from the pool or creates a new one.
        /// </summary>
        /// <typeparam name="T">The operator type to rent</typeparam>
        /// <returns>An instance of the requested operator type</returns>
        /// <exception cref="InvalidOperationException">When <typeparamref name="T"/> is not registered while registration is required, or cannot be created without constructor arguments: it has no public constructor, has multiple public constructors, or has a parameter that is neither injected by this pool nor has a default value</exception>
        /// <exception cref="MissingMethodException">When no public constructor of <typeparamref name="T"/> matches the registered constructor arguments</exception>
        public T Rent<T>() where T : class, IOperator
        {
            return (T)Rent(typeof(T));
        }

        /// <summary>
        /// Rents an operator instance from the pool or creates a new one.
        /// </summary>
        /// <param name="type">The operator type to rent</param>
        /// <returns>An instance of the requested operator type</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="type"/> is null</exception>
        /// <exception cref="InvalidOperationException">When <paramref name="type"/> does not implement <see cref="IOperator"/>, is not registered while registration is required, or cannot be created without constructor arguments: it has no public constructor, has multiple public constructors, or has a parameter that is neither injected by this pool nor has a default value</exception>
        /// <exception cref="MissingMethodException">When no public constructor of <paramref name="type"/> matches the registered constructor arguments</exception>
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

            if (!_registrations.TryGetValue(type, out var args) && _requireRegistration)
            {
                throw new InvalidOperationException($"{type.Name} is not registered.");
            }

            if (args?.Length > 0)
            {
                return (IOperator)Activator.CreateInstance(type, ConvertRegisteredArgs(args));
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
            var resolvedArgs = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                resolvedArgs[i] = ResolveArgument(parameters[i]);
            }

            return (IOperator)constructors[0].Invoke(resolvedArgs);
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
            if (!_registrations.ContainsKey(type) && _requireRegistration)
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
