using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ModelsApi : BaseApi, IModelsApi
    {
        public ModelsApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<List<ApiModel>> GetModelsAsync()
        {
            return GetModelsAsync(CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<List<ApiModel>> GetModelsAsync(CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ApiModelsResponse>(HttpMethod.Get, "models", cancellationToken);
            return response.Data;
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<ApiModel> RetrieveModelAsync(string modelId)
        {
            return RetrieveModelAsync(modelId, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<ApiModel> RetrieveModelAsync(string modelId, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ApiModel>(HttpMethod.Get, $"models/{modelId}", cancellationToken);
            return response;
        }
    }
}
