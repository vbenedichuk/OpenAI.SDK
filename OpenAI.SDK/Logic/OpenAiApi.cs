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
    internal class OpenAiApi : BaseApi, IOpenAiAPI
    {
        public OpenAiApi(IHttpClientFactory clientFactory,
            IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        public async Task<List<ApiModel>> GetModelsAsync()
        {
            var response = await ExecuteRequest<ApiModelsResponse>(HttpMethod.Get, "models");
            return response.Data;
        }

        public async Task<ApiModel> RetrieveModelAsync(string modelId)
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
