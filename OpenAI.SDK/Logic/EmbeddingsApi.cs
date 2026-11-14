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
            var result = await ExecuteRequest<EmbeddingsRequest, EmbeddingsResponse>(HttpMethod.Post, "embeddings", request, cancellationToken);
            return result;
        }
    }
}
