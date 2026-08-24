// Copyright (c) 2023-2026 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Runtime.Serialization;

namespace TestHelper.UI.Exceptions
{
    /// <summary>
    /// Detected multiple <c>GameObject</c>s matching the specified criteria.
    /// </summary>
    [Serializable]
    public class MultipleGameObjectsMatchingException : ApplicationException
    {
        public MultipleGameObjectsMatchingException() { }

        public MultipleGameObjectsMatchingException(string message) : base(message) { }

        public MultipleGameObjectsMatchingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MultipleGameObjectsMatchingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
