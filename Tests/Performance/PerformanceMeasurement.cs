// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using Unity.PerformanceTesting;

namespace TestHelper.UI.Performance
{
    // Measure.Method().GC() only accepts synchronous delegates, so async APIs are sampled manually with
    // Measure.Custom: Stopwatch for time and GC.GetAllocatedBytesForCurrentThread for allocations.
    // GC.GetAllocatedBytesForCurrentThread requires .NET Standard 2.1; the assembly's defineConstraints
    // (UNITY_2021_2_OR_NEWER) guarantee it, so no in-code version guard is needed.
    internal static class PerformanceMeasurement
    {
        private const int WarmupCount = 5;
        private const int MeasurementCount = 20;

        public static async UniTask MeasureAsync(Func<UniTask> action)
        {
            var timeGroup = new SampleGroup("Time");
            var allocationGroup = new SampleGroup("GC.Alloc", SampleUnit.Byte);
            var stopwatch = new Stopwatch();

            for (var i = 0; i < WarmupCount; i++)
            {
                await action();
            }

            for (var i = 0; i < MeasurementCount; i++)
            {
                var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();
                stopwatch.Restart();
                await action();
                stopwatch.Stop();
                // Read the counter before Measure.Custom so the sampling machinery's own allocations are not
                // charged to the GC.Alloc sample.
                var allocatedBytesAfter = GC.GetAllocatedBytesForCurrentThread();

                Measure.Custom(timeGroup, stopwatch.Elapsed.TotalMilliseconds);
                Measure.Custom(allocationGroup, allocatedBytesAfter - allocatedBytesBefore);
            }
        }
    }
}
