// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace TestHelper.UI.Performance
{
    // Measure.Method().GC() only accepts synchronous delegates, so async APIs are sampled manually with
    // Measure.Custom: Stopwatch for time and the managed heap size for allocations.
    //
    // The GC.Alloc samples are managed heap growth, not an exact allocation count: the heap grows in pages,
    // so allocations that fit in already-reserved space read as 0. Compare medians across branches rather
    // than reading a sample as "this operation allocated N bytes".
    //
    // GC.GetAllocatedBytesForCurrentThread is not used: it is a stub on Unity's Mono runtime and always
    // returns 0, so it reports "no allocations" for everything instead of failing.
    // ProfilerRecorder(ProfilerCategory.Memory, "GC.Alloc") — what Measure.Method().GC() uses — is not used
    // either: it yields data only while the Profiler is recording, and Profiler.enabled is false in a plain
    // Editor test run (which is also how CI runs these), where it silently reports unrelated values.
    internal static class PerformanceMeasurement
    {
        // The heap grows in pages, so the first iterations report the expansion rather than the steady-state
        // cost. Warm up long enough for the managed heap to settle before sampling.
        private const int WarmupCount = 30;
        private const int MeasurementCount = 20;
        private const int CounterProbeSize = 1_000_000;

        public static async UniTask MeasureAsync(Func<UniTask> action)
        {
            AssumeAllocationCounterIsAlive();

            var timeGroup = new SampleGroup("Time");
            var allocationGroup = new SampleGroup("GC.Alloc", SampleUnit.Byte);
            var stopwatch = new Stopwatch();

            for (var i = 0; i < WarmupCount; i++)
            {
                await action();
            }

            for (var i = 0; i < MeasurementCount; i++)
            {
                // Not forcing a collection per sample: it frees enough space that the following allocations fit
                // without growing the heap, which reads as a flat 0 bytes and destroys the signal entirely.
                var heapBefore = GC.GetTotalMemory(false);
                stopwatch.Restart();
                await action();
                stopwatch.Stop();
                // Read the heap size before Measure.Custom so the sampling machinery's own allocations are not
                // charged to the GC.Alloc sample. A sample turns negative when a collection happens to run
                // inside the window; the median is the statistic to compare.
                var heapAfter = GC.GetTotalMemory(false);

                Measure.Custom(timeGroup, stopwatch.Elapsed.TotalMilliseconds);
                Measure.Custom(allocationGroup, heapAfter - heapBefore);
            }
        }

        private static void AssumeAllocationCounterIsAlive()
        {
            var before = GC.GetTotalMemory(false);
            var probe = new byte[CounterProbeSize];
            var delta = GC.GetTotalMemory(false) - before;
            GC.KeepAlive(probe);

            // Not GetTotalMemory(true): forcing a collection also reclaims unrelated garbage, which lands the
            // delta just below the probe size (995,328 of 1,000,000 observed) and fails a working counter.
            // Half the probe size is the threshold because this only has to separate a live counter from a
            // dead one, and a dead one reports a flat 0.
            Assume.That(delta, Is.GreaterThan(CounterProbeSize / 2),
                "The allocation counter does not move on this runtime, so GC.Alloc samples would all read 0.");
        }
    }
}
