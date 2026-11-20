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

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiResponseFormatException"/> class.
        /// </summary>
        protected OpenAiResponseFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            StatusCode = (HttpStatusCode)info.GetInt32(nameof(StatusCode));
            ResponseContents = info.GetString(nameof(ResponseContents));
            RequestUri = (Uri)info.GetValue(nameof(RequestUri), typeof(Uri));
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate.</param>
        /// <param name="context">The destination for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue(nameof(StatusCode), (int)StatusCode);
            info.AddValue(nameof(ResponseContents), ResponseContents, typeof(string));
            info.AddValue(nameof(RequestUri), RequestUri, typeof(Uri));
            base.GetObjectData(info, context);
        }

        private static string ComposeMessage(HttpStatusCode statusCode, Uri requestUri, string message)
        {
            var location = requestUri != null ? " for " + requestUri.AbsoluteUri : string.Empty;
            return $"{(int)statusCode} {statusCode}{location}: {message}";
        }
    }
}
