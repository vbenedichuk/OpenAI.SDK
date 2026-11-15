using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
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

            var response = await SendAsync(method, resource, content, true, cancellationToken).ConfigureAwait(false);
            EnsureSuccess(response);
            return Deserialize<TResult>(response);
        }

        protected async Task<byte[]> ExecuteRawRequestAsync(HttpMethod method, string resource, CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(method, resource, null, false, cancellationToken).ConfigureAwait(false);
            EnsureSuccess(response);
            return response.Body;
        }

        protected async Task<T> ExecuteMultipartRequestAsync<T>(HttpMethod method, string resource, MultipartFormDataContent content, CancellationToken cancellationToken = default)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            var response = await SendAsync(method, resource, content, true, cancellationToken).ConfigureAwait(false);
            EnsureSuccess(response);
            return Deserialize<T>(response);
        }

        private async Task<RawHttpResponse> SendAsync(HttpMethod method, string resource, HttpContent content, bool acceptJson, CancellationToken cancellationToken)
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
            if (acceptJson)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            request.Content = content;

            using var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName);
            using var requestCts = CreateRequestCancellationTokenSource(cancellationToken);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestCts.Token).ConfigureAwait(false);

            // Buffer the body once: the response stream cannot be read twice, so error bodies and
            // deserialization failures must reuse the same bytes.
            var responseBody = await ReadContentAsync(response.Content, requestCts.Token).ConfigureAwait(false);

            return new RawHttpResponse(
                response.StatusCode,
                responseBody,
                response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri,
                response.Headers,
                response.ReasonPhrase);
        }

        private static async Task<byte[]> ReadContentAsync(HttpContent content, CancellationToken cancellationToken)
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

        private static T Deserialize<T>(RawHttpResponse response)
        {
            if (response.Body.Length == 0)
            {
                return default(T);
            }

            try
            {
                return JsonSerializer.Deserialize<T>(response.Body, _responseSerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new OpenAiApiException(response.StatusCode, response.BodyText, "Failed to deserialize response body.", response.RequestUri, response.Headers, ex);
            }
        }

        private static void EnsureSuccess(RawHttpResponse response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            throw new OpenAiApiException(response.StatusCode, response.BodyText, response.ReasonPhrase, response.RequestUri, response.Headers);
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

        private sealed class RawHttpResponse
        {
            public RawHttpResponse(System.Net.HttpStatusCode statusCode, byte[] body, Uri requestUri, HttpResponseHeaders headers, string reasonPhrase)
            {
                StatusCode = statusCode;
                Body = body ?? Array.Empty<byte>();
                RequestUri = requestUri;
                Headers = headers;
                ReasonPhrase = reasonPhrase;
            }

            public System.Net.HttpStatusCode StatusCode { get; }

            public byte[] Body { get; }

            public Uri RequestUri { get; }

            public HttpResponseHeaders Headers { get; }

            public string ReasonPhrase { get; }

            public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;

            public string BodyText => Body.Length == 0 ? null : Encoding.UTF8.GetString(Body);
        }
    }
}
