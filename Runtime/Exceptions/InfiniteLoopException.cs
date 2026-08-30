// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Runtime.Serialization;

namespace TestHelper.UI.Exceptions
{
    /// <summary>
    /// Detected an infinite loop in monkey testing.
    /// </summary>
    [Serializable]
    public class InfiniteLoopException : Exception
    {
        public InfiniteLoopException() { }

        public InfiniteLoopException(string message) : base(message) { }

        public InfiniteLoopException(string message, Exception innerException) : base(message, innerException) { }

        protected InfiniteLoopException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
