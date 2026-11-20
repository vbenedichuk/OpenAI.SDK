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
    [Obsolete("API is not supported anymore")]
    internal class EnginesApi : BaseApi, IEnginesApi
    {
        public EnginesApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        public async Task<List<EngineInfo>> ListEnginesAsync(CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequest<ListResponse<EngineInfo>>(HttpMethod.Get, "engines", cancellationToken).ConfigureAwait(false);
            return result != null ? result.Data : null;
        }

        public async Task<EngineInfo> RetrieveEngineAsync(string engineId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(engineId))
            {
                throw new ArgumentException("Engine id is required.", nameof(engineId));
            }
            var result = await ExecuteRequest<EngineInfo>(HttpMethod.Get, $"engines/{engineId}", cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
