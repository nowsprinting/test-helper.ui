// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.UI.Random
{
    public interface IRandomString : IRandomizable
    {
        // CA1716 asks to rename Next because it conflicts with the VB.NET "Next" keyword. Not applied:
        // it is published API of this package, so renaming would be a breaking change rather than a diagnostics fix.
#pragma warning disable CA1716
        string Next(RandomStringParameters parameters);
#pragma warning restore CA1716
    }
}
