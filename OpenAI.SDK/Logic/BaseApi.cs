using OpenAI.SDK.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OpenAI.SDK.Configuration;
using Microsoft.Extensions.Options;

namespace OpenAI.SDK.Logic
{
    internal class BaseApi
    {

        protected readonly IHttpClientFactory _clientFactory;
        protected readonly OpenAiOptions _options;
        protected readonly Uri _baseUri;

        public BaseApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options)
        {
            _clientFactory = clientFactory;
            _options = options.Value;
            _baseUri = new Uri(_options.BaseUrl);
        }
        protected async Task<TResult> ExecuteRequest<TResult>(HttpMethod method, string resource)
        {
            return await ExecuteRequest<object, TResult>(method, resource, null);
        }

        protected async Task<TResult> ExecuteRequest<TSource, TResult>(HttpMethod method, string resource, TSource body)
        {
            var url = new Uri(_baseUri, resource);
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TResult>(contentString);
                return result;
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }
    }
}
