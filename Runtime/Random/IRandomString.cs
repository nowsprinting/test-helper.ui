// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace TestHelper.UI.Random
{
    public interface IRandomString : IRandomizable
    {
        string Next(RandomStringParameters parameters);
    }
}
