// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace TestHelper.UI.ScreenshotFilenameStrategies
{
    [TestFixture]
    public class CounterBasedStrategyTest
    {
        [Test]
        public void GetFilename_PrefixSpecified_ReturnsSequentialFilenamesWithPrefix()
        {
            var strategy = new CounterBasedStrategy("prefix");

            var actual = Enumerable.Repeat(0, 5).Select(_ => strategy.GetFilename()).ToList();
            var expected = new List<string>
            {
                "prefix_0001.png",
                "prefix_0002.png",
                "prefix_0003.png",
                "prefix_0004.png",
                "prefix_0005.png"
            };
            Assert.That(actual, Is.EqualTo(expected));
        }


        [Test]
        public void GetFilename_NoPrefixSpecified_ReturnsSequentialFilenamesWithCallerMemberName()
        {
            var strategy = new CounterBasedStrategy();

            var actual = Enumerable.Repeat(0, 5).Select(_ => strategy.GetFilename()).ToList();
            var expected = new List<string>
            {
                $"{nameof(GetFilename_NoPrefixSpecified_ReturnsSequentialFilenamesWithCallerMemberName)}_0001.png",
                $"{nameof(GetFilename_NoPrefixSpecified_ReturnsSequentialFilenamesWithCallerMemberName)}_0002.png",
                $"{nameof(GetFilename_NoPrefixSpecified_ReturnsSequentialFilenamesWithCallerMemberName)}_0003.png",
                $"{nameof(GetFilename_NoPrefixSpecified_ReturnsSequentialFilenamesWithCallerMemberName)}_0004.png",
                $"{nameof(GetFilename_NoPrefixSpecified_ReturnsSequentialFilenamesWithCallerMemberName)}_0005.png"
            };
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
