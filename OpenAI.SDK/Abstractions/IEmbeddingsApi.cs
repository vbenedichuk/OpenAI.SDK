using OpenAI.SDK.Models.Embeddings;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IEmbeddingsApi
    {
        Task<EmbeddingsResponse> CreateEmbeddingsAsync(EmbeddingsRequest request, CancellationToken cancellationToken = default);
    }
}
