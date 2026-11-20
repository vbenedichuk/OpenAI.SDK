using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Completions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{

    [Obsolete]
    internal class CompletionsApi : BaseApi, ICompletionsApi
    {
        public CompletionsApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        /// <inheritdoc/>
        public async Task<ApiCompletionResponse> CreateCompletionAsync(ApiCompletionRequest apiCompletionRequest, CancellationToken cancellationToken = default)
        {
            if (apiCompletionRequest == null)
            {
                throw new ArgumentNullException(nameof(apiCompletionRequest));
            }
            if (string.IsNullOrWhiteSpace(apiCompletionRequest.Model))
            {
                throw new ArgumentException("Model is required.", nameof(apiCompletionRequest));
            }
            if (apiCompletionRequest.Stream == true)
            {
                throw new NotSupportedException("Streaming responses are not supported by this SDK.");
            }

            var response = await ExecuteRequest<ApiCompletionRequest, ApiCompletionResponse>(HttpMethod.Post, "completions", apiCompletionRequest, cancellationToken).ConfigureAwait(false);
            return response;
        }        
    }
}
