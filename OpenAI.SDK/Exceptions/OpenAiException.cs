using System;
using System.Runtime.Serialization;

namespace OpenAI.SDK.Exceptions
{
    /// <summary>
    /// Base class for ApenAI related exceptions.
    /// </summary>
    [Serializable]
    public class OpenAiException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiException"/> class.
        /// </summary>
        public OpenAiException() : base()
        {
        }

        public OpenAiException(string message) : base(message)
        {
        }

        public OpenAiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OpenAiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
