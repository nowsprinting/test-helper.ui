// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using TestHelper.UI.Operators;
using TestHelper.UI.Strategies;
using TestHelper.UI.TestDoubles;
using TestHelper.UI.Visualizers;
using UnityEngine;

namespace TestHelper.UI
{
    [TestFixture]
    public class OperatorPoolTest
    {
        [Test]
        public void Rent_RegisteredTypeWithNoArgs_ReturnsNewInstance()
        {
            var pool = new OperatorPool();
            pool.Register<UguiClickOperator>();

            var instance = pool.Rent<UguiClickOperator>();

            Assert.That(instance, Is.InstanceOf<UguiClickOperator>());
        }

        [Test]
        public void Rent_RegisteredTypeWithArgs_ReturnsInstanceCreatedWithArgs()
        {
            const int IntValue = 42;
            var logger = new SpyLogger();
            var screenshotOptions = new ScreenshotOptions();
            var visualizer = new DefaultDebugVisualizer();
            Func<GameObject, Vector2> getScreenPoint = _ => Vector2.zero;
            var reachableStrategy = new DefaultReachableStrategy();
            var random = new SpyRandom();

            var pool = new OperatorPool();
            pool.Register<FakeOperator>(
                IntValue, logger, screenshotOptions, visualizer, getScreenPoint, reachableStrategy, random);

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance, Is.InstanceOf<FakeOperator>());
            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
            Assert.That(instance.Logger, Is.SameAs(logger));
            Assert.That(instance.ScreenshotOptions, Is.SameAs(screenshotOptions));
            Assert.That(instance.Visualizer, Is.SameAs(visualizer));
            Assert.That(instance.GetScreenPoint, Is.SameAs(getScreenPoint));
            Assert.That(instance.ReachableStrategy, Is.SameAs(reachableStrategy));
            Assert.That(((SpyRandom)instance.Random).ForkedFrom, Is.SameAs(random)); // forked instance
        }

        [Test]
        public void Rent_RegisteredTypeWithoutPublicConstructor_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();
            pool.Register<FakeOperatorWithoutPublicConstructor>();

            Assert.That(() => pool.Rent<FakeOperatorWithoutPublicConstructor>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("FakeOperatorWithoutPublicConstructor has no public constructor."));
        }

        [Test]
        public void Rent_PoolIsEmpty_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();

            Assert.That(() => pool.Rent<UguiClickOperator>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("UguiClickOperator is not registered."));
        }

        [Test]
        public void Rent_UnregisteredType_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();
            pool.Register<UguiClickAndHoldOperator>();

            Assert.That(() => pool.Rent<UguiClickOperator>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("UguiClickOperator is not registered."));
        }

        [Test]
        public void Rent_AfterReturn_ReturnsSameInstance()
        {
            var pool = new OperatorPool();
            pool.Register<UguiClickOperator>();

            var instance1 = pool.Rent<UguiClickOperator>();
            pool.Return(instance1);

            var instance2 = pool.Rent<UguiClickOperator>();
            Assert.That(instance2, Is.SameAs(instance1));
        }

        [Test]
        public void RentAll_ReturnsAllOperatorInstances()
        {
            var pool = new OperatorPool();
            pool.Register<UguiClickAndHoldOperator>();
            pool.Register<UguiClickOperator>();

            var operators = pool.RentAll().ToArray();
            var operatorTypes = operators.Select(instance => instance.GetType()).ToArray();
            Assert.That(operatorTypes, Is.EquivalentTo(new[]
            {
                typeof(UguiClickAndHoldOperator),
                typeof(UguiClickOperator)
            }));
        }

        [Test]
        public void Return_NullInstance_ThrowsArgumentNullException()
        {
            var pool = new OperatorPool();

            Assert.That(() => pool.Return(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Return_UnregisteredType_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();

            var unregisteredInstance = new UguiClickOperator();

            Assert.That(() => pool.Return(unregisteredInstance),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("UguiClickOperator is not registered."));
        }

        [Test]
        public void Register_CalledTwice_OverwritesPreviousRegistration()
        {
            const int IntValue = 42;
            var logger = new SpyLogger();
            var screenshotOptions = new ScreenshotOptions();
            var visualizer = new DefaultDebugVisualizer();

            var pool = new OperatorPool();
            pool.Register<FakeOperator>(); // dummy
            pool.Register<FakeOperator>(IntValue, logger, screenshotOptions, visualizer, null, null, null);

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance, Is.InstanceOf<FakeOperator>());
            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
            Assert.That(instance.Logger, Is.SameAs(logger));
            Assert.That(instance.ScreenshotOptions, Is.SameAs(screenshotOptions));
            Assert.That(instance.Visualizer, Is.SameAs(visualizer));
        }

        [Test]
        public void Rent_PoolConstructedWithLogger_InjectsLoggerIntoOperator()
        {
            var logger = new SpyLogger();
            var pool = new OperatorPool(logger: logger);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.Logger, Is.SameAs(logger));
        }

        [Test]
        public void Rent_PoolConstructedWithScreenshotOptions_InjectsScreenshotOptionsIntoOperator()
        {
            var screenshotOptions = new ScreenshotOptions();
            var pool = new OperatorPool(screenshotOptions: screenshotOptions);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.ScreenshotOptions, Is.SameAs(screenshotOptions));
        }

        [Test]
        [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
        public void Rent_PoolConstructedWithVisualizer_InjectsVisualizerIntoOperator()
        {
            var visualizer = new DefaultDebugVisualizer();
            var pool = new OperatorPool(visualizer: visualizer);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.Visualizer, Is.SameAs(visualizer));
        }

        [Test]
        public void Rent_PoolConstructedWithGetScreenPoint_InjectsGetScreenPointIntoOperator()
        {
            Func<GameObject, Vector2> getScreenPoint = _ => Vector2.zero;
            var pool = new OperatorPool(getScreenPoint: getScreenPoint);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.GetScreenPoint, Is.SameAs(getScreenPoint));
        }

        [Test]
        public void Rent_PoolConstructedWithReachableStrategy_InjectsReachableStrategyIntoOperator()
        {
            var reachableStrategy = new DefaultReachableStrategy();
            var pool = new OperatorPool(reachableStrategy: reachableStrategy);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.ReachableStrategy, Is.SameAs(reachableStrategy));
        }

        [Test]
        public void Rent_PoolConstructedWithRandom_InjectsForkedRandomIntoOperator()
        {
            var random = new SpyRandom();
            var pool = new OperatorPool(random: random);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(((SpyRandom)instance.Random).ForkedFrom, Is.SameAs(random));
        }

        [Test]
        [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
        public void Rent_PoolConstructedWithDependencies_InjectsDependenciesIntoOperator()
        {
            var logger = new SpyLogger();
            var screenshotOptions = new ScreenshotOptions();
            var visualizer = new DefaultDebugVisualizer();
            Func<GameObject, Vector2> getScreenPoint = _ => Vector2.zero;
            var reachableStrategy = new DefaultReachableStrategy();
            var random = new SpyRandom();
            var pool = new OperatorPool(
                logger: logger,
                screenshotOptions: screenshotOptions,
                visualizer: visualizer,
                getScreenPoint: getScreenPoint,
                reachableStrategy: reachableStrategy,
                random: random);
            pool.Register<FakeOperator>(); // no args

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.Logger, Is.SameAs(logger));
            Assert.That(instance.ScreenshotOptions, Is.SameAs(screenshotOptions));
            Assert.That(instance.Visualizer, Is.SameAs(visualizer));
            Assert.That(instance.GetScreenPoint, Is.SameAs(getScreenPoint));
            Assert.That(instance.ReachableStrategy, Is.SameAs(reachableStrategy));
            Assert.That(((SpyRandom)instance.Random).ForkedFrom, Is.SameAs(random)); // forked instance
        }

        [Test]
        public void Rent_RegisteredTypeWithArgs_DoesNotInjectDependencies()
        {
            var loggerInPools = new SpyLogger();
            var loggerInArgs = new SpyLogger();
            var pool = new OperatorPool(logger: loggerInPools);
            pool.Register<FakeOperator>(0, loggerInArgs, null, null, null, null, null);

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance.Logger, Is.SameAs(loggerInArgs));
        }

        [Test]
        public void Rent_RegisteredTypeWithRequiredUnresolvableParameter_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();
            pool.Register<FakeOperatorWithRequiredParam>();

            Assert.That(() => pool.Rent<FakeOperatorWithRequiredParam>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "Cannot resolve required parameter 'requiredParam' of type String. " +
                        "Register with explicit constructor arguments or add a default value."));
        }
    }
}
