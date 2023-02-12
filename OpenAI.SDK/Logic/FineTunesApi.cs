using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.FineTunes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class FineTunesApi : BaseApi, IFineTunesApi
    {
        public FineTunesApi(IHttpClientFactory clientFactory, IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        public async Task<FineTune> CancelFineTuneAsync(string fineTuneId)
        {
            var response = await ExecuteRequest<FineTune>(HttpMethod.Post, $"fine-tunes/{fineTuneId}/cancel");
            return response;
        }

        public async Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request)
        {
            var response = await ExecuteRequest<CreateFineTuneRequest, FineTune>(HttpMethod.Post, "fine-tunes", request);
            return response;
        }

        public async Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model)
        {
            var response = await ExecuteRequest<DeleteFineTuneResponse>(HttpMethod.Delete, $"models/{model}");
            return response;
        }

        public async Task<List<FineTuneEvent>> ListFineTuneEvents(string fineTuneId, bool? stream = null)
        {
            var response = await ExecuteRequest<ListResponse<FineTuneEvent>>(HttpMethod.Get, $"fine-tunes/{fineTuneId}/events");
            return response.Data;
        }

        public async Task<List<FineTune>> ListFineTunesAsync()
        {
            var response = await ExecuteRequest<ListResponse<FineTune>>(HttpMethod.Get, $"fine-tunes");
            return response.Data;
        }

        public async Task<FineTune> RetrieveFineTuneAsync(string fineTuneId)
        {
            var response = await ExecuteRequest<FineTune>(HttpMethod.Get, $"fine-tunes/{fineTuneId}");
            return response;
        }
    }
}
