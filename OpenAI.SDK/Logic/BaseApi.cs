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
        private const int CopyBufferSize = 81920;
        private const int ErrorBodyLimit = 64 * 1024;

        private static readonly JsonSerializerOptions _requestSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly JsonSerializerOptions _responseSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

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
            HttpContent content = null;
            if (!EqualityComparer<TSource>.Default.Equals(body, default))
            {
                content = JsonContent.Create(body, options: _requestSerializerOptions);
            }

            return await ExecuteJsonRequestAsync<TResult>(method, resource, content, cancellationToken).ConfigureAwait(false);
        }

        protected async Task<byte[]> ExecuteRawRequestAsync(HttpMethod method, string resource, CancellationToken cancellationToken = default)
        {
            ValidateRequest(method, resource);
            var requestUri = BuildRequestUri(resource);
            using var request = CreateRequest(method, requestUri, null, false);
            using var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName);
            using var requestCts = CreateRequestCancellationTokenSource(cancellationToken);

            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestCts.Token).ConfigureAwait(false);
                var resolvedRequestUri = response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri;

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await ReadContentAsStringAsync(response.Content, requestCts.Token, ErrorBodyLimit).ConfigureAwait(false);
                    throw new OpenAiApiException(response.StatusCode, errorBody, response.ReasonPhrase, resolvedRequestUri, response.Headers);
                }

                return await ReadAllBytesAsync(response.Content, requestCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && requestCts.IsCancellationRequested)
            {
                throw new OpenAiTimeoutException("The OpenAI request timed out.", ex);
            }
        }

        protected async Task<T> ExecuteMultipartRequestAsync<T>(HttpMethod method, string resource, MultipartFormDataContent content, CancellationToken cancellationToken = default)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return await ExecuteJsonRequestAsync<T>(method, resource, content, cancellationToken).ConfigureAwait(false);
        }

        private async Task<TResult> ExecuteJsonRequestAsync<TResult>(HttpMethod method, string resource, HttpContent content, CancellationToken cancellationToken)
        {
            ValidateRequest(method, resource);
            var requestUri = BuildRequestUri(resource);
            using var request = CreateRequest(method, requestUri, content, true);
            using var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName);
            using var requestCts = CreateRequestCancellationTokenSource(cancellationToken);

            try
            {
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestCts.Token).ConfigureAwait(false);
                var resolvedRequestUri = response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri;

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await ReadContentAsStringAsync(response.Content, requestCts.Token, ErrorBodyLimit).ConfigureAwait(false);
                    throw new OpenAiApiException(response.StatusCode, errorBody, response.ReasonPhrase, resolvedRequestUri, response.Headers);
                }

                if (IsEmptyResponse(response))
                {
                    return default;
                }

                if (response.Content == null)
                {
                    return default;
                }

                var contentLength = response.Content.Headers.ContentLength;
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    if (stream == null)
                    {
                        return default;
                    }

                    if (!contentLength.HasValue)
                    {
                        using (var buffer = new MemoryStream())
                        {
                            await stream.CopyToAsync(buffer, CopyBufferSize, requestCts.Token).ConfigureAwait(false);
                            if (buffer.Length == 0)
                            {
                                return default;
                            }

                            buffer.Position = 0;
                            try
                            {
                                return await JsonSerializer.DeserializeAsync<TResult>(buffer, _responseSerializerOptions, requestCts.Token).ConfigureAwait(false);
                            }
                            catch (JsonException ex)
                            {
                                throw new OpenAiResponseFormatException(response.StatusCode, null, "Failed to deserialize response body.", resolvedRequestUri, ex);
                            }
                        }
                    }

                    try
                    {
                        return await JsonSerializer.DeserializeAsync<TResult>(stream, _responseSerializerOptions, requestCts.Token).ConfigureAwait(false);
                    }
                    catch (JsonException ex)
                    {
                        throw new OpenAiResponseFormatException(response.StatusCode, null, "Failed to deserialize response body.", resolvedRequestUri, ex);
                    }
                }
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && requestCts.IsCancellationRequested)
            {
                throw new OpenAiTimeoutException("The OpenAI request timed out.", ex);
            }
        }

        private static bool IsEmptyResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }

            return response.Content == null || response.Content.Headers.ContentLength == 0;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, Uri requestUri, HttpContent content, bool acceptJson)
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            if (acceptJson)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            request.Content = content;
            return request;
        }

        private static void ValidateRequest(HttpMethod method, string resource)
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
        }

        private static async Task<byte[]> ReadAllBytesAsync(HttpContent content, CancellationToken cancellationToken)
        {
            if (content == null)
            {
                return Array.Empty<byte>();
            }

            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                if (stream == null)
                {
                    return Array.Empty<byte>();
                }

                using (var buffer = new MemoryStream())
                {
                    var chunk = new byte[CopyBufferSize];
                    int read;
                    while ((read = await stream.ReadAsync(chunk, 0, chunk.Length, cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        buffer.Write(chunk, 0, read);
                    }
                    return buffer.ToArray();
                }
            }
        }

        private static async Task<string> ReadContentAsStringAsync(HttpContent content, CancellationToken cancellationToken, int maxBytes)
        {
            if (content == null)
            {
                return null;
            }

            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var buffer = new MemoryStream())
                {
                    var chunk = new byte[CopyBufferSize];
                    int read;
                    long total = 0;
                    while (total < maxBytes && (read = await stream.ReadAsync(chunk, 0, (int)Math.Min(chunk.Length, maxBytes - total), cancellationToken).ConfigureAwait(false)) > 0)
                    {
                        buffer.Write(chunk, 0, read);
                        total += read;
                    }

                    if (buffer.Length == 0)
                    {
                        return null;
                    }

                    return Encoding.UTF8.GetString(buffer.ToArray());
                }
            }
        }

        private CancellationTokenSource CreateRequestCancellationTokenSource(CancellationToken cancellationToken)
        {
            var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (_options.Timeout > TimeSpan.Zero)
            {
                requestCts.CancelAfter(_options.Timeout);
            }
            return requestCts;
        }

        private Uri BuildRequestUri(string resource)
        {
            // Escape each path segment while preserving '/' separators and any query string.
            // Callers must pass raw (unescaped) values: already escaped input is escaped again.
            var path = resource;
            var query = string.Empty;
            var queryIndex = resource.IndexOf('?');
            if (queryIndex >= 0)
            {
                path = resource.Substring(0, queryIndex);
                query = resource.Substring(queryIndex);
            }

            var segments = path.Split('/');
            for (var i = 0; i < segments.Length; i++)
            {
                if (segments[i] == "." || segments[i] == "..")
                {
                    throw new ArgumentException("Resource path must not contain '.' or '..' segments.", nameof(resource));
                }
                if (segments[i].Length > 0)
                {
                    segments[i] = Uri.EscapeDataString(segments[i]);
                }
            }

            var resolved = new Uri(_baseUri, string.Join("/", segments) + query);
            if (!_baseUri.IsBaseOf(resolved))
            {
                throw new ArgumentException("Resource path must stay within the configured BaseUrl.", nameof(resource));
            }

            return resolved;
        }
    }
}
