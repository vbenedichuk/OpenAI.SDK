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
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class FineTunesApi : BaseApi, IFineTunesApi
    {
        public FineTunesApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<FineTune> CancelFineTuneAsync(string fineTuneId)
        {
            return CancelFineTuneAsync(fineTuneId, CancellationToken.None);
        }

        public async Task<FineTune> CancelFineTuneAsync(string fineTuneId, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<FineTune>(HttpMethod.Post, $"fine-tunes/{fineTuneId}/cancel", cancellationToken);
            return response;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request)
        {
            return CreateFineTuneAsync(request, CancellationToken.None);
        }

        public async Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<CreateFineTuneRequest, FineTune>(HttpMethod.Post, "fine-tunes", request, cancellationToken);
            return response;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model)
        {
            return DeleteFineTuneModelAsync(model, CancellationToken.None);
        }

        public async Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<DeleteFineTuneResponse>(HttpMethod.Delete, $"models/{model}", cancellationToken);
            return response;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<List<FineTuneEvent>> ListFineTuneEvents(string fineTuneId, bool? stream = null)
        {
            return ListFineTuneEventsAsync(fineTuneId, stream, CancellationToken.None);
        }

        public async Task<List<FineTuneEvent>> ListFineTuneEventsAsync(string fineTuneId, bool? stream = null, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ListResponse<FineTuneEvent>>(HttpMethod.Get, $"fine-tunes/{fineTuneId}/events", cancellationToken);
            return response.Data;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<List<FineTune>> ListFineTunesAsync()
        {
            return ListFineTunesAsync(CancellationToken.None);
        }

        public async Task<List<FineTune>> ListFineTunesAsync(CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ListResponse<FineTune>>(HttpMethod.Get, $"fine-tunes", cancellationToken);
            return response.Data;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<FineTune> RetrieveFineTuneAsync(string fineTuneId)
        {
            return RetrieveFineTuneAsync(fineTuneId, CancellationToken.None);
        }

        public async Task<FineTune> RetrieveFineTuneAsync(string fineTuneId, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<FineTune>(HttpMethod.Get, $"fine-tunes/{fineTuneId}", cancellationToken);
            return response;
        }
    }
}
