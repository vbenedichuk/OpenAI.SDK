using System;
using System.Net;
using System.Runtime.Serialization;

namespace OpenAI.SDK.Exceptions
{
    /// <summary>
    /// Base class for ApenAI API related exceptions.
    /// </summary>
    [Serializable]
    public class OpenAiApiException : OpenAiException
    {
        /// <summary>
        /// Provides HTTP response status code
        /// </summary>
        public virtual HttpStatusCode StatusCode { get; protected set; }
        
        /// <summary>
        /// Provides HTTP Response contents.
        /// </summary>
        public virtual string ResponseContents { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        public OpenAiApiException(HttpStatusCode statusCode, string responseContents, string message) : this(statusCode, responseContents, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        public OpenAiApiException(HttpStatusCode statusCode, string responseContents, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
            ResponseContents = responseContents;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        protected OpenAiApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
