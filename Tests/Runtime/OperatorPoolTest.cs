// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.UI.Operators;
using TestHelper.UI.Visualizers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TestHelper.UI
{
    [TestFixture]
    public class OperatorPoolTest
    {
        // Test dummy operator with default constructor parameters
        private class DummyOperatorWithDefaults : IOperator
        {
            public ILogger Logger { get; set; }
            public ScreenshotOptions ScreenshotOptions { get; set; }
            public IVisualizer Visualizer { get; set; }

            public int Value { get; }

            public DummyOperatorWithDefaults(int value = 42)
            {
                Value = value;
            }

            public bool CanOperate(GameObject gameObject) => false;

            public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
                CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        }

        // Test dummy operator with required constructor parameters
        private class DummyOperatorWithArgs : IOperator
        {
            public ILogger Logger { get; set; }
            public ScreenshotOptions ScreenshotOptions { get; set; }
            public IVisualizer Visualizer { get; set; }

            public string Name { get; }
            public int Count { get; }

            public DummyOperatorWithArgs(string name, int count)
            {
                Name = name;
                Count = count;
            }

            public bool CanOperate(GameObject gameObject) => false;

            public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
                CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        }

        // Test dummy operator without registration (for error cases)
        private class UnregisteredOperator : IOperator
        {
            public ILogger Logger { get; set; }
            public ScreenshotOptions ScreenshotOptions { get; set; }
            public IVisualizer Visualizer { get; set; }

            public bool CanOperate(GameObject gameObject) => false;

            public UniTask OperateAsync(GameObject gameObject, RaycastResult raycastResult = default,
                CancellationToken cancellationToken = default) => UniTask.CompletedTask;
        }

        [Test]
        public void Rent_RegisteredTypeWithNoArgs_ReturnsNewInstance()
        {
            var pool = new OperatorPool();
            pool.Register<DummyOperatorWithDefaults>();

            var instance = pool.Rent<DummyOperatorWithDefaults>();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance, Is.InstanceOf<DummyOperatorWithDefaults>());
            Assert.That(instance.Value, Is.EqualTo(42));
        }

        [Test]
        public void Rent_RegisteredTypeWithArgs_ReturnsInstanceCreatedWithArgs()
        {
            var pool = new OperatorPool();
            pool.Register<DummyOperatorWithArgs>("test", 123);

            var instance = pool.Rent<DummyOperatorWithArgs>();

            Assert.That(instance, Is.Not.Null);
            Assert.That(instance.Name, Is.EqualTo("test"));
            Assert.That(instance.Count, Is.EqualTo(123));
        }

        [Test]
        public void Rent_UnregisteredType_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();

            Assert.Throws<InvalidOperationException>(() => pool.Rent<UnregisteredOperator>());
        }

        [Test]
        public void Rent_AfterReturn_ReturnsSameInstance()
        {
            var pool = new OperatorPool();
            pool.Register<DummyOperatorWithDefaults>();

            var instance1 = pool.Rent<DummyOperatorWithDefaults>();
            pool.Return(instance1);
            var instance2 = pool.Rent<DummyOperatorWithDefaults>();

            Assert.That(instance2, Is.SameAs(instance1));
        }

        [Test]
        public void Return_NullInstance_ThrowsArgumentNullException()
        {
            var pool = new OperatorPool();

            Assert.Throws<ArgumentNullException>(() => pool.Return(null));
        }

        [Test]
        public void Return_UnregisteredType_ThrowsInvalidOperationException()
        {
            var pool = new OperatorPool();
            var instance = new UnregisteredOperator();

            Assert.Throws<InvalidOperationException>(() => pool.Return(instance));
        }

        [Test]
        public void Register_CalledTwice_OverwritesPreviousRegistration()
        {
            var pool = new OperatorPool();
            pool.Register<DummyOperatorWithArgs>("first", 100);
            pool.Register<DummyOperatorWithArgs>("second", 200);

            var instance = pool.Rent<DummyOperatorWithArgs>();

            Assert.That(instance.Name, Is.EqualTo("second"));
            Assert.That(instance.Count, Is.EqualTo(200));
        }
    }
}
