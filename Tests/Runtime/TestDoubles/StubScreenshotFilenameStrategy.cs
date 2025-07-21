// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using TestHelper.UI.ScreenshotFilenameStrategies;

namespace TestHelper.UI.TestDoubles
{
    public class StubScreenshotFilenameStrategy : IScreenshotFilenameStrategy
    {
        private readonly string _screenshotFilename;

        public StubScreenshotFilenameStrategy(string screenshotFilename)
        {
            _screenshotFilename = screenshotFilename;
        }

        public string GetFilename()
        {
            return _screenshotFilename;
        }
    }
}
