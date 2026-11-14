using OpenAI.SDK.Models.Embeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IEmbeddingsApi
    {
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<EmbeddingsResponse> CreateEmbeddings(EmbeddingsRequest request);

        Task<EmbeddingsResponse> CreateEmbeddingsAsync(EmbeddingsRequest request, CancellationToken cancellationToken = default);
    }
}
