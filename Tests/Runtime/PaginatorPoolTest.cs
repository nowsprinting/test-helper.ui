// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using TestHelper.UI.Paginators;
using TestHelper.UI.TestDoubles;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
// UnityEngine.TestTools.Constraints is imported for the AllocatingGCMemory extension method, which brings a
// second `Is` into scope. Aliased to NUnit's so that the other assertions in this file keep resolving to it.
using Is = NUnit.Framework.Is;

namespace TestHelper.UI
{
    [TestFixture]
    public class PaginatorPoolTest
    {
        [Test]
        public void Rent_RegisteredTypeWithNoArgs_ReturnsNewInstance()
        {
            var pool = new PaginatorPool();
            pool.Register<UguiScrollRectPaginator>();

            var instance = pool.Rent<UguiScrollRectPaginator>();

            Assert.That(instance, Is.InstanceOf<UguiScrollRectPaginator>());
        }

        [Test]
        public void Rent_RegisteredTypeWithArgs_ReturnsInstanceCreatedWithArgs()
        {
            const int IntValue = 42;
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>(IntValue);

            var instance = pool.Rent<FakePaginator>();

            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
        }

        [Test]
        public void Rent_RegisteredTypeWithoutPublicConstructor_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool();
            pool.Register<FakePaginatorWithoutPublicConstructor>();

            Assert.That(() => pool.Rent<FakePaginatorWithoutPublicConstructor>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("FakePaginatorWithoutPublicConstructor has no public constructor."));
        }

        [Test]
        public void Rent_RegisteredTypeWithMultiplePublicConstructorsAndNoArgs_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool();
            pool.Register<FakePaginatorWithMultiplePublicConstructors>();

            Assert.That(() => pool.Rent<FakePaginatorWithMultiplePublicConstructors>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "FakePaginatorWithMultiplePublicConstructors has multiple public constructors. " +
                        "Register with explicit constructor arguments."));
        }

        [Test]
        public void Rent_RegisteredTypeWithMultiplePublicConstructorsAndArgs_ReturnsInstanceCreatedWithArgs()
        {
            const int IntValue = 42;
            var pool = new PaginatorPool();
            pool.Register<FakePaginatorWithMultiplePublicConstructors>(IntValue);

            var instance = pool.Rent<FakePaginatorWithMultiplePublicConstructors>();

            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
        }

        [Test]
        public void Rent_RegisteredTypeWithRequiredParameter_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool();
            pool.Register<FakePaginatorWithRequiredParam>();

            Assert.That(() => pool.Rent<FakePaginatorWithRequiredParam>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo(
                        "Cannot resolve required parameter 'requiredParam' of type String. " +
                        "Register with explicit constructor arguments or add a default value."));
        }

        [Test]
        public void Rent_UnregisteredType_NoRegistrations_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool();

            Assert.That(() => pool.Rent<UguiScrollRectPaginator>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("UguiScrollRectPaginator is not registered."));
        }

        [Test]
        public void Rent_UnregisteredType_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool();
            pool.Register<UguiScrollbarPaginator>();

            Assert.That(() => pool.Rent<UguiScrollRectPaginator>(),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("UguiScrollRectPaginator is not registered."));
        }

        [Test]
        public void Rent_UnregisteredType_NotRequireRegistration_ReturnsNewInstance()
        {
            var pool = new PaginatorPool(requireRegistration: false);

            var instance = pool.Rent<UguiScrollRectPaginator>();

            Assert.That(instance, Is.InstanceOf<UguiScrollRectPaginator>());
        }

        [Test]
        public void Rent_NullType_ThrowsArgumentNullException()
        {
            var pool = new PaginatorPool(requireRegistration: false);

            Assert.That(() => pool.Rent(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Rent_TypeNotImplementingIPaginator_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool(requireRegistration: false);

            Assert.That(() => pool.Rent(typeof(string)), Throws.InvalidOperationException);
        }

        [Test]
        [Category("Acceptance")]
        public void Rent_AfterReturn_ReturnsSameInstance()
        {
            var pool = new PaginatorPool();
            pool.Register<UguiScrollRectPaginator>();

            var instance1 = pool.Rent<UguiScrollRectPaginator>();
            pool.Return(instance1);

            var instance2 = pool.Rent<UguiScrollRectPaginator>();
            Assert.That(instance2, Is.Not.Null.And.SameAs(instance1));
        }

        [Test]
        public void Rent_AfterReturn_DoesNotAllocateGCMemory()
        {
            var pool = new PaginatorPool();
            pool.Register<UguiScrollRectPaginator>();
            var instance = pool.Rent<UguiScrollRectPaginator>();
            Assume.That(instance, Is.Not.Null);
            pool.Return(instance);

            Assert.That(() => { pool.Rent<UguiScrollRectPaginator>(); }, Is.Not.AllocatingGCMemory());
        }

        [Test]
        public void Rent_AfterReturn_UnregisteredType_NotRequireRegistration_ReturnsSameInstance()
        {
            var pool = new PaginatorPool(requireRegistration: false);

            var instance1 = pool.Rent<UguiScrollRectPaginator>();
            pool.Return(instance1);

            var instance2 = pool.Rent<UguiScrollRectPaginator>();
            Assert.That(instance2, Is.Not.Null.And.SameAs(instance1));
        }

        [Test]
        [CreateScene]
        [Category("Acceptance")]
        public void Rent_WithTargetComponent_NewInstanceHasTargetComponentAssigned()
        {
            var targetComponent = new GameObject("Target").AddComponent<FakeComponent>();
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>();

            var instance = pool.Rent<FakePaginator>(targetComponent);

            Assert.That(instance.TargetComponent, Is.SameAs(targetComponent));
        }

        [Test]
        [CreateScene]
        public void Rent_TypeWithTargetComponent_ReturnsInstanceOfTypeWithTargetComponentAssigned()
        {
            var targetComponent = new GameObject("Target").AddComponent<FakeComponent>();
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>();

            var instance = pool.Rent(typeof(FakePaginator), targetComponent);

            Assert.That(instance, Is.InstanceOf<FakePaginator>());
            Assert.That(((FakePaginator)instance).TargetComponent, Is.SameAs(targetComponent));
        }

        [Test]
        [CreateScene]
        [Category("Acceptance")]
        public void Rent_WithTargetComponent_AfterReturn_PooledInstanceHasTargetComponentAssigned()
        {
            var firstTargetComponent = new GameObject("First").AddComponent<FakeComponent>();
            var secondTargetComponent = new GameObject("Second").AddComponent<FakeComponent>();
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>();
            var instance1 = pool.Rent<FakePaginator>(firstTargetComponent);
            pool.Return(instance1);

            var instance2 = pool.Rent<FakePaginator>(secondTargetComponent);

            Assert.That(instance2, Is.SameAs(instance1));
            Assert.That(instance2.TargetComponent, Is.SameAs(secondTargetComponent));
        }

        [Test]
        [Category("Acceptance")]
        public void Rent_WithoutTargetComponent_TargetComponentIsNull()
        {
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>();

            var instance = pool.Rent<FakePaginator>();

            Assert.That(instance.TargetComponent, Is.Null);
        }

        [Test]
        public void Return_NullInstance_ThrowsArgumentNullException()
        {
            var pool = new PaginatorPool();

            Assert.That(() => pool.Return(null), Throws.ArgumentNullException);
        }

        [Test]
        public void Return_UnregisteredType_ThrowsInvalidOperationException()
        {
            var pool = new PaginatorPool();

            var unregisteredInstance = new FakePaginator();

            Assert.That(() => pool.Return(unregisteredInstance),
                Throws.InvalidOperationException
                    .With.Message.EqualTo("FakePaginator is not registered."));
        }

        [Test]
        [CreateScene]
        public void Return_RentedInstanceWithTargetComponent_TargetComponentIsCleared()
        {
            var targetComponent = new GameObject("Target").AddComponent<FakeComponent>();
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>();
            var instance = pool.Rent<FakePaginator>(targetComponent);

            pool.Return(instance);

            Assert.That(instance.TargetComponent, Is.Null);
        }

        [Test]
        public void Register_CalledTwice_OverwritesPreviousRegistration()
        {
            const int IntValue = 42;
            var pool = new PaginatorPool();
            pool.Register<FakePaginator>(); // dummy
            pool.Register<FakePaginator>(IntValue);

            var instance = pool.Rent<FakePaginator>();

            Assert.That(instance.IntValue, Is.EqualTo(IntValue));
        }
    }
}
