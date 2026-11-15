using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class BaseApi
    {
        private static readonly JsonSerializerOptions _requestSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions _responseSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public const string HttpClientName = ConfigurationExtensions.HttpClientName;

        protected readonly IHttpClientFactory _clientFactory;
        protected readonly OpenAiOptions _options;
        protected readonly Uri _baseUri;

        public BaseApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _options = options != null ? (options.Value ?? throw new ArgumentNullException(nameof(options))) : throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new ArgumentException("OpenAI API key is required. Configure OpenAiOptions.ApiKey (for example via user secrets or environment configuration) before using the SDK.", nameof(options));
            }

            var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
                ? "https://api.openai.com/v1/"
                : _options.BaseUrl;

            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) ||
                (baseUri.Scheme != Uri.UriSchemeHttp && baseUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("OpenAI BaseUrl must be an absolute HTTP(S) URL, for example 'https://api.openai.com/v1/'.", nameof(options));
            }

            _baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/");
        }

        protected async Task<TResult> ExecuteRequest<TResult>(HttpMethod method, string resource, CancellationToken cancellationToken = default)
        {
            return await ExecuteRequest<object, TResult>(method, resource, null, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<TResult> ExecuteRequest<TSource, TResult>(HttpMethod method, string resource, TSource body, CancellationToken cancellationToken = default)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentException("Resource path is required.", nameof(resource));
            }
            if (Uri.TryCreate(resource, UriKind.Absolute, out _))
            {
                throw new ArgumentException("Resource path must be relative; absolute URIs are not allowed.", nameof(resource));
            }

            var requestUri = BuildRequestUri(resource);
            using var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!EqualityComparer<TSource>.Default.Equals(body, default))
            {
                request.Content = JsonContent.Create(body, options: _requestSerializerOptions);
            }

            using var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName);
            if (_options.Timeout > TimeSpan.Zero)
            {
                client.Timeout = _options.Timeout;
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var responseRequestUri = response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri;

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = null;
                try
                {
                    errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                throw new OpenAiApiException(response.StatusCode, errorBody, response.ReasonPhrase, responseRequestUri, response.Headers);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(TResult);
            }

            var contentLength = response.Content.Headers.ContentLength;
            if (contentLength.HasValue && contentLength.Value == 0)
            {
                return default(TResult);
            }

            try
            {
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    if (stream == null)
                    {
                        return default(TResult);
                    }
                    // Detect empty stream without buffering the whole body.
                    if (stream.CanSeek && stream.Length == 0)
                    {
                        return default(TResult);
                    }
                    return await JsonSerializer.DeserializeAsync<TResult>(stream, _responseSerializerOptions, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (JsonException ex)
            {
                string responseBody = null;
                try
                {
                    responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                throw new OpenAiApiException(response.StatusCode, responseBody, "Failed to deserialize response body.", responseRequestUri, response.Headers, ex);
            }
        }

        protected async Task<byte[]> ExecuteRawRequestAsync(HttpMethod method, string resource, CancellationToken cancellationToken = default)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentException("Resource path is required.", nameof(resource));
            }
            if (Uri.TryCreate(resource, UriKind.Absolute, out _))
            {
                throw new ArgumentException("Resource path must be relative; absolute URIs are not allowed.", nameof(resource));
            }

            var requestUri = BuildRequestUri(resource);
            using var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName);
            if (_options.Timeout > TimeSpan.Zero)
            {
                client.Timeout = _options.Timeout;
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var responseRequestUri = response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri;

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = null;
                try
                {
                    errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                throw new OpenAiApiException(response.StatusCode, errorBody, response.ReasonPhrase, responseRequestUri, response.Headers);
            }

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);
                return memoryStream.ToArray();
            }
        }

        protected async Task<T> ExecuteMultipartRequestAsync<T>(HttpMethod method, string resource, MultipartFormDataContent content, CancellationToken cancellationToken = default)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (string.IsNullOrWhiteSpace(resource))
            {
                throw new ArgumentException("Resource path is required.", nameof(resource));
            }
            if (Uri.TryCreate(resource, UriKind.Absolute, out _))
            {
                throw new ArgumentException("Resource path must be relative; absolute URIs are not allowed.", nameof(resource));
            }
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var requestUri = BuildRequestUri(resource);
            using var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            request.Content = content;

            using var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName);
            if (_options.Timeout > TimeSpan.Zero)
            {
                client.Timeout = _options.Timeout;
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            var responseRequestUri = response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri;

            if (!response.IsSuccessStatusCode)
            {
                string errorBody = null;
                try
                {
                    errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                throw new OpenAiApiException(response.StatusCode, errorBody, response.ReasonPhrase, responseRequestUri, response.Headers);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            var contentLength = response.Content.Headers.ContentLength;
            if (contentLength.HasValue && contentLength.Value == 0)
            {
                return default(T);
            }

            try
            {
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    return await JsonSerializer.DeserializeAsync<T>(stream, _responseSerializerOptions, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (JsonException ex)
            {
                string responseBody = null;
                try
                {
                    responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                throw new OpenAiApiException(response.StatusCode, responseBody, "Failed to deserialize response body.", responseRequestUri, response.Headers, ex);
            }
        }

        private Uri BuildRequestUri(string resource)
        {
            // Escape each path segment while preserving '/' separators and any query string.
            string path = resource;
            string query = string.Empty;
            int queryIndex = resource.IndexOf('?');
            if (queryIndex >= 0)
            {
                path = resource.Substring(0, queryIndex);
                query = resource.Substring(queryIndex);
            }

            var segments = path.Split('/');
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i].Length > 0)
                {
                    segments[i] = Uri.EscapeDataString(segments[i]);
                }
            }
            var escaped = string.Join("/", segments) + query;
            return new Uri(_baseUri, escaped);
        }
    }
}