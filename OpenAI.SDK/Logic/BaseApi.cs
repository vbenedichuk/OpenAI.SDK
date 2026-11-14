using OpenAI.SDK.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenAI.SDK.Configuration;
using Microsoft.Extensions.Options;

namespace OpenAI.SDK.Logic
{
    internal class BaseApi
    {

        private static readonly JsonSerializerOptions _requestSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        protected readonly IHttpClientFactory _clientFactory;
        protected readonly OpenAiOptions _options;
        protected readonly Uri _baseUri;

        public BaseApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options)
        {
            _clientFactory = clientFactory;
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));

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
            return await ExecuteRequest<object, TResult>(method, resource, null, cancellationToken);
        }

        protected async Task<TResult> ExecuteRequest<TSource, TResult>(HttpMethod method, string resource, TSource body, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, resource);
            using var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            if (!EqualityComparer<TSource>.Default.Equals(body, default))
            {
                request.Content = JsonContent.Create(body, options: _requestSerializerOptions);
            }
            using var client = _clientFactory.CreateClient();
            using var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<TResult>(stream, cancellationToken: cancellationToken);
                }
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }
    }
}
