// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.UI.Operators;
using TestHelper.UI.TestDoubles;
using TestHelper.UI.Visualizers;

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

            var pool = new OperatorPool();
            pool.Register<FakeOperator>(IntValue, logger, screenshotOptions, visualizer);

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance, Is.InstanceOf<FakeOperator>());
            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
            Assert.That(instance.Logger, Is.SameAs(logger));
            Assert.That(instance.ScreenshotOptions, Is.SameAs(screenshotOptions));
            Assert.That(instance.Visualizer, Is.SameAs(visualizer));
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
            pool.Register<FakeOperator>(IntValue, logger, screenshotOptions, visualizer);

            var instance = pool.Rent<FakeOperator>();

            Assert.That(instance, Is.InstanceOf<FakeOperator>());
            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
            Assert.That(instance.Logger, Is.SameAs(logger));
            Assert.That(instance.ScreenshotOptions, Is.SameAs(screenshotOptions));
            Assert.That(instance.Visualizer, Is.SameAs(visualizer));
        }
    }
}
