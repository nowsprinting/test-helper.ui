// Copyright (c) 2023-2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Attributes;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace TestHelper.UI.Performance
{
    public class MonkeyTest
    {
        private const string MeasurePackageVersion = "0.13.3";

        [Test]
        [Performance, Version(MeasurePackageVersion)]
        [LoadScene("../Scenes/Operators.unity")]
        public void GetLotteryEntries_GotAllInteractableComponentAndOperators()
        {
            var config = new MonkeyConfig();
            var finder = new InteractableComponentsFinder(config.IsInteractable, config.OperatorPool);

            Measure.Method(() =>
                {
                    // ReSharper disable once IteratorMethodResultIsIgnored
                    Monkey.GetLotteryEntries(finder);
                })
                .WarmupCount(5)
                .MeasurementCount(20)
                .GC()
                .Run();
        }

        [Test]
        [Performance, Version(MeasurePackageVersion)]
        [LoadScene("../Scenes/PhysicsRaycasterSandbox.unity")]
        public void LotteryOperator_BingoReachableComponent()
        {
            var config = new MonkeyConfig();
            var finder = new InteractableComponentsFinder(config.IsInteractable, config.OperatorPool);
            var operators = Monkey.GetLotteryEntries(finder);

            Measure.Method(() =>
                {
                    Monkey.LotteryOperator(operators, config.Random, config.IgnoreStrategy, config.ReachableStrategy);
                })
                .WarmupCount(5)
                .MeasurementCount(20)
                .GC()
                .Run();
        }

        [UnityTest]
        [Performance, Version(MeasurePackageVersion)]
        [LoadScene("../Scenes/Operators.unity")]
        public IEnumerator RunStep_finish()
        {
            var config = new MonkeyConfig();
            var finder = new InteractableComponentsFinder(config.IsInteractable, config.OperatorPool);

            using (Measure.Frames().Scope())
            {
                yield return Monkey.RunStep(
                        config.Random,
                        config.Logger,
                        finder,
                        config.IgnoreStrategy,
                        config.ReachableStrategy)
                    .ToCoroutine();
            }
        }
    }
}
