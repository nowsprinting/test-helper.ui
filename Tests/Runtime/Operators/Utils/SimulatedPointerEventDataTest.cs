// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace TestHelper.UI.Operators.Utils
{
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP017:Prefer using")]
    public class SimulatedPointerEventDataTest
    {
        [SetUp]
        public void SetUp()
        {
            FingerPool.Instance.Reset();
        }

        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
        public void Constructor_WithoutPointingDeviceTypeInEditor_AsMouse()
        {
            var sut = new SimulatedPointerEventData(null, default);
            Assert.That(sut.pointerId, Is.EqualTo(-1));
            sut.Dispose();
        }

        [Test]
        public void Constructor_WithMouse_SetPointerIdIsLeftButton()
        {
            var sut = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.Mouse);
            Assert.That(sut.pointerId, Is.EqualTo(-1));
            sut.Dispose();
        }

        [Test]
        public void Constructor_WithTouchScreen_IncrementTouchCount()
        {
            var firstTouch = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assume.That(firstTouch.pointerId, Is.EqualTo(0));

            var secondTouch = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assert.That(secondTouch.pointerId, Is.EqualTo(1));

            firstTouch.Dispose();
            secondTouch.Dispose();
        }

        [Test]
        public void Constructor_WithTouchScreen_ReuseReleasedPointerId()
        {
            var finger0 = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assume.That(finger0.pointerId, Is.EqualTo(0));

            var finger1 = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assume.That(finger1.pointerId, Is.EqualTo(1));

            finger0.Dispose();

            var finger2 = new SimulatedPointerEventData(null, default,
                SimulatedPointerEventData.PointingDeviceType.TouchScreen);
            Assert.That(finger2.pointerId, Is.EqualTo(0));

            finger1.Dispose();
            finger2.Dispose();
        }
    }
}
