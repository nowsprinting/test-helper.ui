// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;

namespace TestHelper.UI.Operators.Utils
{
    [TestFixture]
    public class FingerPoolTest
    {
        [SetUp]
        public void SetUp()
        {
            FingerPool.Instance.Reset();
        }

        [Test]
        public void Acquire_FirstCall_ReturnsZero()
        {
            var actual = FingerPool.Instance.Acquire();

            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void Acquire_SecondCall_ReturnsOne()
        {
            FingerPool.Instance.Acquire();

            var actual = FingerPool.Instance.Acquire();

            Assert.That(actual, Is.EqualTo(1));
        }

        [Test]
        public void Acquire_AfterReleaseFirstId_ReusesReleasedId()
        {
            var id0 = FingerPool.Instance.Acquire();
            var id1 = FingerPool.Instance.Acquire();
            FingerPool.Instance.Release(id0);

            var actual = FingerPool.Instance.Acquire();

            Assert.That(actual, Is.EqualTo(0));
        }

        [Test]
        public void Acquire_AfterReleaseMiddleId_ReusesReleasedId()
        {
            FingerPool.Instance.Acquire();
            var id1 = FingerPool.Instance.Acquire();
            FingerPool.Instance.Acquire();
            FingerPool.Instance.Release(id1);

            var actual = FingerPool.Instance.Acquire();

            Assert.That(actual, Is.EqualTo(1));
        }
    }
}
