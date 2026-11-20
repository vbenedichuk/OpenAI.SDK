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
        private readonly string _apiKey;

        public BaseApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options)
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _options = options != null ? (options.Value ?? throw new ArgumentNullException(nameof(options))) : throw new ArgumentNullException(nameof(options));

            // Shared with startup validation so both paths reject the same configuration.
            _baseUri = OpenAiOptionsValidator.Validate(_options);
            _apiKey = OpenAiOptionsValidator.NormalizeApiKey(_options.ApiKey);
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

        protected Task<byte[]> ExecuteRawRequestAsync(HttpMethod method, string resource, CancellationToken cancellationToken = default)
        {
            return ExecuteHttpRequestAsync(method, resource, null, false, cancellationToken, (response, _, token) => ReadAllBytesAsync(response.Content, token));
        }

        protected async Task<T> ExecuteMultipartRequestAsync<T>(HttpMethod method, string resource, MultipartFormDataContent content, CancellationToken cancellationToken = default)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            return await ExecuteJsonRequestAsync<T>(method, resource, content, cancellationToken).ConfigureAwait(false);
        }

        // The success handler runs only for 2xx responses and must not dispose the response.
        // Cancellation observed while the SDK timeout is the trigger is reported as OpenAiTimeoutException.
        private async Task<TResult> ExecuteHttpRequestAsync<TResult>(HttpMethod method, string resource, HttpContent content, bool acceptJson, CancellationToken cancellationToken, Func<HttpResponseMessage, Uri, CancellationToken, Task<TResult>> handleSuccessAsync)
        {
            ValidateRequest(method, resource);
            var requestUri = BuildRequestUri(resource);
            using (var request = CreateRequest(method, requestUri, content, acceptJson))
            using (var client = _clientFactory.CreateClient(ConfigurationExtensions.HttpClientName))
            using (var requestCts = CreateRequestCancellationTokenSource(cancellationToken))
            {
                var resolvedRequestUri = requestUri;
                try
                {
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, requestCts.Token).ConfigureAwait(false))
                    {
                        resolvedRequestUri = response.RequestMessage != null ? response.RequestMessage.RequestUri : requestUri;

                        if (!response.IsSuccessStatusCode)
                        {
                            var errorBody = await ReadContentAsStringAsync(response.Content, requestCts.Token, ErrorBodyLimit).ConfigureAwait(false);
                            throw new OpenAiApiException(response.StatusCode, errorBody, response.ReasonPhrase, resolvedRequestUri, response.Headers);
                        }

                        return await handleSuccessAsync(response, resolvedRequestUri, requestCts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && requestCts.IsCancellationRequested)
                {
                    throw new OpenAiTimeoutException(resolvedRequestUri, _options.Timeout, ex);
                }
            }
        }

        private Task<TResult> ExecuteJsonRequestAsync<TResult>(HttpMethod method, string resource, HttpContent content, CancellationToken cancellationToken)
        {
            return ExecuteHttpRequestAsync(method, resource, content, true, cancellationToken, (response, resolvedRequestUri, token) => ReadJsonResponseAsync<TResult>(response, resolvedRequestUri, token));
        }

        private static async Task<TResult> ReadJsonResponseAsync<TResult>(HttpResponseMessage response, Uri resolvedRequestUri, CancellationToken cancellationToken)
        {
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
                        await stream.CopyToAsync(buffer, CopyBufferSize, cancellationToken).ConfigureAwait(false);
                        if (buffer.Length == 0)
                        {
                            return default;
                        }

                        buffer.Position = 0;
                        try
                        {
                            return await JsonSerializer.DeserializeAsync<TResult>(buffer, _responseSerializerOptions, cancellationToken).ConfigureAwait(false);
                        }
                        catch (JsonException ex)
                        {
                            var body = GetCapturedText(buffer, ErrorBodyLimit);
                            throw new OpenAiResponseFormatException(response.StatusCode, body, "Failed to deserialize response body.", resolvedRequestUri, ex);
                        }
                    }
                }

                using (var captured = new CapturingStream(stream, ErrorBodyLimit))
                {
                    try
                    {
                        return await JsonSerializer.DeserializeAsync<TResult>(captured, _responseSerializerOptions, cancellationToken).ConfigureAwait(false);
                    }
                    catch (JsonException ex)
                    {
                        throw new OpenAiResponseFormatException(response.StatusCode, captured.CapturedText, "Failed to deserialize response body.", resolvedRequestUri, ex);
                    }
                }
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
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
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
                        await buffer.WriteAsync(chunk, 0, read, cancellationToken);
                    }
                    return buffer.ToArray();
                }
            }
        }

        private static string GetCapturedText(MemoryStream buffer, int maxBytes)
        {
            if (buffer.Length == 0)
            {
                return null;
            }

            var length = (int)Math.Min(buffer.Length, maxBytes);
            return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, length);
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
                        await buffer.WriteAsync(chunk, 0, read, cancellationToken);
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
            var timeout = _options.Timeout;
            if (timeout > TimeSpan.Zero)
            {
                // CancelAfter rejects delays above the platform timer maximum; clamp instead of throwing.
                if (timeout.TotalMilliseconds > int.MaxValue)
                {
                    timeout = TimeSpan.FromMilliseconds(int.MaxValue);
                }
                requestCts.CancelAfter(timeout);
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

        private sealed class CapturingStream : Stream
        {
            private readonly Stream _inner;
            private readonly MemoryStream _capture = new MemoryStream();
            private readonly int _limit;

            public CapturingStream(Stream inner, int limit)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
                _limit = limit;
            }

            public string CapturedText => GetCapturedText(_capture, _limit);

            public override bool CanRead => _inner.CanRead;

            public override bool CanSeek => _inner.CanSeek;

            public override bool CanWrite => false;

            public override long Length => _inner.Length;

            public override long Position
            {
                get => _inner.Position;
                set => _inner.Position = value;
            }

            public override void Flush() => _inner.Flush();

            public override int Read(byte[] buffer, int offset, int count)
            {
                var read = _inner.Read(buffer, offset, count);
                Capture(buffer, offset, read);
                return read;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                Capture(buffer, offset, read);
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _capture.Dispose();
                }
                base.Dispose(disposing);
            }

            private void Capture(byte[] buffer, int offset, int count)
            {
                if (count <= 0)
                {
                    return;
                }

                var remaining = _limit - (int)_capture.Length;
                if (remaining <= 0)
                {
                    return;
                }

                _capture.Write(buffer, offset, Math.Min(count, remaining));
            }
        }
    }
}
