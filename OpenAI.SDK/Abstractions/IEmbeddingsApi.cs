using OpenAI.SDK.Models.Embeddings;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IEmbeddingsApi
    {
        Task<EmbeddingsResponse> CreateEmbeddings(EmbeddingsRequest request);
    }
}
