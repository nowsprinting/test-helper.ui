// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System.Runtime.CompilerServices;

namespace TestHelper.UI.ScreenshotFilenameStrategies
{
    /// <summary>
    ///     Sequential number based screenshot file path strategy.
    /// </summary>
    // InconsistentNaming asks this to end with the base class name "AbstractPrefixAndUniqueIDStrategy". Not
    // applied: it is published API of this package, so renaming would be a breaking change, and the suggested
    // name would be an unreadable concatenation anyway.
#pragma warning disable THP3002
    public class CounterBasedStrategy : AbstractPrefixAndUniqueIDStrategy
#pragma warning restore THP3002
    {
        private int _count;


        public CounterBasedStrategy(string filenamePrefix = null, [CallerMemberName] string callerMemberName = null) :
            base(filenamePrefix, callerMemberName)
        {
        }


        protected override string GetUniqueID()
        {
            return (++_count).ToString("D04");
        }
    }
}
