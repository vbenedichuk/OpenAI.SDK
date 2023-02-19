using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ModelsApi : BaseApi, IModelsApi
    {
        public ModelsApi(IHttpClientFactory clientFactory, IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        /// </<inheritdoc/>
        public async Task<List<ApiModel>> GetModelsAsync()
        {
            var response = await ExecuteRequest<ApiModelsResponse>(HttpMethod.Get, "models");
            return response.Data;
        }

        /// </<inheritdoc/>
        public async Task<ApiModel> RetrieveModelAsync(string modelId)
        {
            var response = await ExecuteRequest<ApiModel>(HttpMethod.Get, $"models/{modelId}");
            return response;
        }
    }
}
