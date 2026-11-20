using System;

namespace OpenAI.SDK.Configuration
{
    /// <summary>
    /// Configuration for the OpenAI SDK.
    /// </summary>
    public class OpenAiOptions
    {
        /// <summary>
        /// OpenAI API key. Sent as a Bearer token and required for every request.
        /// Leading and trailing whitespace is ignored.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Base address of the API. Defaults to <c>https://api.openai.com/v1/</c>.
        /// Must be an absolute HTTP(S) URL without a query string or fragment.
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

        /// <summary>
        /// Maximum duration of a single request, including reading the response body.
        /// Defaults to 100 seconds. Set to <see cref="TimeSpan.Zero"/> or
        /// <see cref="System.Threading.Timeout.InfiniteTimeSpan"/> to disable the limit.
        /// This value takes precedence over <c>HttpClient.Timeout</c>.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
    }
}
