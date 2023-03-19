using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Chat;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ChatApi : BaseApi, IChatApi
    {
        public ChatApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
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
