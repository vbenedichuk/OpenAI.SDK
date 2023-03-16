using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Chat;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ChatApi : BaseApi, IChat
    {
        public ChatApi(IHttpClientFactory clientFactory,
            IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }
        /// <inheritdoc/>
        public async Task<ApiChatCompletionResponse> CreateCompletionAsync(ApiChatCompletionRequest apiChatCompletionRequest)
        {
            var response = await ExecuteRequest<ApiChatCompletionRequest, ApiChatCompletionResponse>(HttpMethod.Post, "chat/completions", apiChatCompletionRequest);
            return response;
        }
    }
}
