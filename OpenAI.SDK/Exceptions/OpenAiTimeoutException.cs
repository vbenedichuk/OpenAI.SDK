using System;
using System.Runtime.Serialization;

namespace OpenAI.SDK.Exceptions
{
    /// <summary>
    /// Thrown when an OpenAI SDK request exceeds the configured timeout.
    /// </summary>
    [Serializable]
    public class OpenAiTimeoutException : OpenAiException
    {
        public OpenAiTimeoutException()
            : base("The OpenAI request timed out.")
        {
        }

        public OpenAiTimeoutException(string message)
            : base(message)
        {
        }

        public OpenAiTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected OpenAiTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
