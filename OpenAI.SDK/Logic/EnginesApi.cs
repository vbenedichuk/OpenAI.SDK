using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Engines;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class EnginesApi : BaseApi, IEnginesApi
    {
        public EnginesApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        public async Task<List<EngineInfo>> ListEnginesAsync()
        {
            var result = await ExecuteRequest<ListResponse<EngineInfo>>(HttpMethod.Get, "engines");
            return result.Data;
        }

        public async Task<EngineInfo> RetrieveEngine(string engineId)
        {
            var result = await ExecuteRequest<EngineInfo>(HttpMethod.Get, $"engines/{engineId}");
            return result;
        }
    }
}
