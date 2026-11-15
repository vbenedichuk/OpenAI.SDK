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
        /// <summary>
        /// The request URI, when available.
        /// </summary>
        public virtual Uri RequestUri { get; protected set; }

        /// <summary>
        /// The timeout that elapsed, or <see cref="TimeSpan.Zero"/> when unknown.
        /// </summary>
        public virtual TimeSpan Timeout { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiTimeoutException"/> class.
        /// </summary>
        public OpenAiTimeoutException()
            : base("The OpenAI request timed out.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiTimeoutException"/> class.
        /// </summary>
        public OpenAiTimeoutException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiTimeoutException"/> class.
        /// </summary>
        public OpenAiTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiTimeoutException"/> class
        /// carrying request context.
        /// </summary>
        public OpenAiTimeoutException(Uri requestUri, TimeSpan timeout, Exception innerException)
            : base(ComposeMessage(requestUri, timeout), innerException)
        {
            RequestUri = requestUri;
            Timeout = timeout;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiTimeoutException"/> class.
        /// </summary>
        protected OpenAiTimeoutException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string ComposeMessage(Uri requestUri, TimeSpan timeout)
        {
            var location = requestUri != null ? " to " + requestUri.AbsoluteUri : string.Empty;
            return $"The OpenAI request{location} timed out after {timeout}.";
        }
    }
}
