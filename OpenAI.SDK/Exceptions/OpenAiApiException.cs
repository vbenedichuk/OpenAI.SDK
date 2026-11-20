using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;

namespace OpenAI.SDK.Exceptions
{
    /// <summary>
    /// Base class for OpenAI API related exceptions.
    /// </summary>
    [Serializable]
    public class OpenAiApiException : OpenAiException
    {
        private const int MaxBodyLength = 1024;

        /// <summary>
        /// Provides HTTP response status code
        /// </summary>
        public virtual HttpStatusCode StatusCode { get; protected set; }

        /// <summary>
        /// Provides HTTP Response contents.
        /// </summary>
        public virtual string ResponseContents { get; protected set; }

        /// <summary>
        /// Provides the request URI, when available.
        /// </summary>
        public virtual Uri RequestUri { get; protected set; }

        /// <summary>
        /// Provides the response headers, when available.
        /// </summary>
        public virtual HttpResponseHeaders ResponseHeaders { get; protected set; }

        /// <summary>
        /// Provides the value of the Retry-After header, when present.
        /// </summary>
        public virtual TimeSpan? RetryAfter { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        public OpenAiApiException(HttpStatusCode statusCode, string responseContents, string message)
            : this(statusCode, responseContents, message, null, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        public OpenAiApiException(HttpStatusCode statusCode, string responseContents, string message, Exception innerException)
            : this(statusCode, responseContents, message, null, null, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        public OpenAiApiException(HttpStatusCode statusCode, string responseContents, string message, Uri requestUri, HttpResponseHeaders responseHeaders)
            : this(statusCode, responseContents, message, requestUri, responseHeaders, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        public OpenAiApiException(HttpStatusCode statusCode, string responseContents, string message, Uri requestUri, HttpResponseHeaders responseHeaders, Exception innerException)
            : base(ComposeMessage(statusCode, requestUri, message, responseContents), innerException)
        {
            StatusCode = statusCode;
            ResponseContents = responseContents;
            RequestUri = requestUri;
            ResponseHeaders = responseHeaders;
            RetryAfter = responseHeaders != null ? GetRetryAfter(responseHeaders) : null;
        }

        private static string ComposeMessage(HttpStatusCode statusCode, Uri requestUri, string message, string responseContents)
        {
            var builder = new StringBuilder();
            builder.Append((int)statusCode);
            builder.Append(' ');
            builder.Append(statusCode);
            if (requestUri != null)
            {
                builder.Append(" for ");
                builder.Append(requestUri.AbsoluteUri);
            }
            if (!string.IsNullOrWhiteSpace(message) && !IsRedundantReasonPhrase(message, statusCode))
            {
                builder.Append(": ");
                builder.Append(message);
            }
            if (!string.IsNullOrWhiteSpace(responseContents))
            {
                builder.Append(" Response: ");
                builder.Append(Truncate(responseContents, MaxBodyLength));
            }
            return builder.ToString();
        }

        private static bool IsRedundantReasonPhrase(string message, HttpStatusCode statusCode)
        {
            // The status line already contains the code and its name ("401 Unauthorized"),
            // so an identical reason phrase must not be repeated in the message.
            return string.Equals(
                message.Replace(" ", string.Empty).Trim(),
                statusCode.ToString(),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }
            return value.Substring(0, maxLength) + "... (truncated)";
        }

        private static TimeSpan? GetRetryAfter(HttpResponseHeaders headers)
        {
            if (headers.RetryAfter == null)
            {
                return null;
            }
            if (headers.RetryAfter.Delta.HasValue)
            {
                return headers.RetryAfter.Delta;
            }
            if (headers.RetryAfter.Date.HasValue)
            {
                var delta = headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                return delta > TimeSpan.Zero ? delta : TimeSpan.Zero;
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
        /// </summary>
        /// <remarks>
        /// <see cref="ResponseHeaders"/> is not serialized because <see cref="HttpResponseHeaders"/>
        /// is not serializable; <see cref="RetryAfter"/> is preserved instead.
        /// </remarks>
        protected OpenAiApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            StatusCode = (HttpStatusCode)info.GetInt32(nameof(StatusCode));
            ResponseContents = info.GetString(nameof(ResponseContents));
            RequestUri = (Uri)info.GetValue(nameof(RequestUri), typeof(Uri));
            RetryAfter = (TimeSpan?)info.GetValue(nameof(RetryAfter), typeof(TimeSpan));
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
            info.AddValue(nameof(RetryAfter), RetryAfter, typeof(TimeSpan));
            base.GetObjectData(info, context);
        }
    }
}
