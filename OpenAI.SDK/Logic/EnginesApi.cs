using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Engines;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class EnginesApi : BaseApi, IEnginesApi
    {
        public EnginesApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<List<EngineInfo>> ListEnginesAsync()
        {
            return ListEnginesAsync(CancellationToken.None);
        }

        public async Task<List<EngineInfo>> ListEnginesAsync(CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequest<ListResponse<EngineInfo>>(HttpMethod.Get, "engines", cancellationToken);
            return result.Data;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<EngineInfo> RetrieveEngine(string engineId)
        {
            return RetrieveEngineAsync(engineId, CancellationToken.None);
        }

        public async Task<EngineInfo> RetrieveEngineAsync(string engineId, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequest<EngineInfo>(HttpMethod.Get, $"engines/{engineId}", cancellationToken);
            return result;
        }
    }
}
