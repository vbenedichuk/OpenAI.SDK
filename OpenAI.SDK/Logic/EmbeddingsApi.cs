using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Embeddings;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class EmbeddingsApi : BaseApi, IEmbeddingsApi
    {
        public EmbeddingsApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<EmbeddingsResponse> CreateEmbeddings(EmbeddingsRequest request)
        {
            return CreateEmbeddingsAsync(request, CancellationToken.None);
        }

        public async Task<EmbeddingsResponse> CreateEmbeddingsAsync(EmbeddingsRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrWhiteSpace(request.Model))
            {
                throw new ArgumentException("Model is required.", nameof(request));
            }
            if (request.Input == null || request.Input.Length == 0)
            {
                throw new ArgumentException("Input is required.", nameof(request));
            }
            var result = await ExecuteRequest<EmbeddingsRequest, EmbeddingsResponse>(HttpMethod.Post, "embeddings", request, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
