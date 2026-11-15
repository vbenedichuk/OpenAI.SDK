using System;

namespace OpenAI.SDK.Configuration
{
    /// <summary>
    /// Validates <see cref="OpenAiOptions"/> and normalizes the values used by the SDK.
    /// </summary>
    internal static class OpenAiOptionsValidator
    {
        /// <summary>
        /// Error text used when the API key is missing.
        /// </summary>
        public const string ApiKeyRequiredMessage =
            "OpenAI API key is required. Configure OpenAiOptions.ApiKey (for example via user secrets or environment configuration) before using the SDK.";

        /// <summary>
        /// Error text used when the base URL is invalid.
        /// </summary>
        public const string BaseUrlInvalidMessage =
            "OpenAI BaseUrl must be an absolute HTTP(S) URL without a query string or fragment, for example 'https://api.openai.com/v1/'.";

        /// <summary>
        /// Validates the options and returns the normalized base URI.
        /// </summary>
        /// <exception cref="ArgumentException">The options are invalid.</exception>
        public static Uri Validate(OpenAiOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                throw new ArgumentException(ApiKeyRequiredMessage, nameof(options));
            }

            var baseUrl = string.IsNullOrWhiteSpace(options.BaseUrl)
                ? "https://api.openai.com/v1/"
                : options.BaseUrl;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) ||
                (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps) ||
                !string.IsNullOrEmpty(baseUri.Query) ||
                !string.IsNullOrEmpty(baseUri.Fragment))
            {
                throw new ArgumentException(BaseUrlInvalidMessage, nameof(options));
            }

            return new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/");
        }

        /// <summary>
        /// Removes leading and trailing whitespace from the API key.
        /// </summary>
        public static string NormalizeApiKey(string apiKey)
        {
            return apiKey == null ? null : apiKey.Trim();
        }
    }
}
