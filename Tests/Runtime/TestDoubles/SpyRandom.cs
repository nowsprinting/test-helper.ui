// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using TestHelper.Random;
using UnityEngine;

namespace TestHelper.UI.TestDoubles
{
    public class SpyRandom : IRandom
    {
        public IRandom ForkedFrom { get; private set; }

        public IRandom Fork()
        {
            return new SpyRandom() { ForkedFrom = this };
        }

        #region not implemented

        public void InitState(int seed)
        {
            throw new NotImplementedException();
        }

        public UnityEngine.Random.State state { get; set; }

        public float Range(float minInclusive, float maxInclusive)
        {
            throw new NotImplementedException();
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            throw new NotImplementedException();
        }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public float value { get; }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public Vector3 insideUnitSphere { get; }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public Vector2 insideUnitCircle { get; }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public Vector3 onUnitSphere { get; }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public Quaternion rotation { get; }

        [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
        public Quaternion rotationUniform { get; }

        public Color ColorHSV(float hueMin = 0, float hueMax = 1, float saturationMin = 0, float saturationMax = 1,
            float valueMin = 0,
            float valueMax = 1, float alphaMin = 1, float alphaMax = 1)
        {
            throw new NotImplementedException();
        }

        public int Next()
        {
            throw new NotImplementedException();
        }

        public int Next(int maxValue)
        {
            throw new NotImplementedException();
        }

        public int Next(int minValue, int maxValue)
        {
            throw new NotImplementedException();
        }

        public void NextBytes(byte[] buffer)
        {
            throw new NotImplementedException();
        }

#if UNITY_2021_2_OR_NEWER
        public void NextBytes(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }
#endif

        public double NextDouble()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
