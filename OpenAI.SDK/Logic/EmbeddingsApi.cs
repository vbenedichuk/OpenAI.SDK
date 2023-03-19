using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Embeddings;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class EmbeddingsApi : BaseApi, IEmbeddingsApi
    {
        public EmbeddingsApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }
        public async Task<EmbeddingsResponse> CreateEmbeddings(EmbeddingsRequest request)
        {
            var result = await ExecuteRequest<EmbeddingsResponse>(HttpMethod.Post, "embeddings");
            return result;
        }
    }
}
