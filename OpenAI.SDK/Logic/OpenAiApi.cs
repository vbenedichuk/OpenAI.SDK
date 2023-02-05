using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.Edits;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class OpenAiApi : IOpenAiAPI
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly OpenApiOptions _options;
        private readonly Uri _baseUri;
        public OpenAiApi(IHttpClientFactory clientFactory,
            IOptions<OpenApiOptions> options)
        {
            _clientFactory = clientFactory;
            _options = options.Value;
            _baseUri = new Uri(_options.BaseUrl);
        }

        private async Task<TResult> ExecuteRequest<TResult>(HttpMethod method, string resource)
        {
            return await ExecuteRequest<object, TResult>(method, resource, null);
        }
        
        private async Task<TResult> ExecuteRequest<TSource, TResult>(HttpMethod method, string resource, TSource body)
        {
            var url = new Uri(_baseUri, resource);
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            if (body != null)
            {
                request.Content = JsonContent.Create(body, options: new JsonSerializerOptions
                {
                    IgnoreNullValues = true
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

        public async Task<List<ApiModel>> GetModelsAsync()
        {
            var response = await ExecuteRequest<ApiModelsResponse>(HttpMethod.Get, "models");
            return response.Data;
        }

        public async Task<ApiModel> GetModelAsync(string modelId)
        {
            var response = await ExecuteRequest<ApiModel>(HttpMethod.Get, $"models/{modelId}");
            return response;
        }

        public async Task<ApiCompletionResponse> CreateCompletionAsync(ApiCompletionRequest apiCompletionRequest)
        {
            var response = await ExecuteRequest<ApiCompletionRequest, ApiCompletionResponse>(HttpMethod.Post, "completions", apiCompletionRequest);
            return response;
        }


        public async Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest)
        {
            var response = await ExecuteRequest<ApiCreateEditRequest, ApiCreateEditRespoinse>(HttpMethod.Post, "edits", createEditRequest);
            return response;
        }

        
    }
}
