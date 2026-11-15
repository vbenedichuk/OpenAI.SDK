using System;
using System.Net;
using System.Runtime.Serialization;

namespace OpenAI.SDK.Exceptions
{
    /// <summary>
    /// Thrown when the OpenAI API returns a successful response whose body cannot be deserialized
    /// into the expected type.
    /// </summary>
    [Serializable]
    public class OpenAiResponseFormatException : OpenAiException
    {
        /// <summary>
        /// The HTTP status code returned by the server.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The raw response body, when available.
        /// </summary>
        public string ResponseContents { get; }

        /// <summary>
        /// The request URI, when available.
        /// </summary>
        public Uri RequestUri { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiResponseFormatException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code returned by the server.</param>
        /// <param name="responseContents">The captured response body, or its first 64 KB.</param>
        /// <param name="message">A description of the failure.</param>
        /// <param name="requestUri">The request URI, when available.</param>
        /// <param name="innerException">The original deserialization exception.</param>
        public OpenAiResponseFormatException(HttpStatusCode statusCode, string responseContents, string message, Uri requestUri, Exception innerException)
            : base(ComposeMessage(statusCode, requestUri, message), innerException)
        {
            StatusCode = statusCode;
            ResponseContents = responseContents;
            RequestUri = requestUri;
        }

        protected OpenAiResponseFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static string ComposeMessage(HttpStatusCode statusCode, Uri requestUri, string message)
        {
            var location = requestUri != null ? " for " + requestUri.AbsoluteUri : string.Empty;
            return $"{(int)statusCode} {statusCode}{location}: {message}";
        }
    }
}
