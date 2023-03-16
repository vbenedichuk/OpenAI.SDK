using OpenAI.SDK.Models.Chat;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IChat
    {
        /// <summary>
        /// POST https://api.openai.com/v1/chat/completions 
        /// Creates a completion for the chat message
        /// </summary>
        /// <param name="apiCompletionRequest"></param>
        /// <returns></returns>
        Task<ApiChatCompletionResponse> CreateCompletionAsync(ApiChatCompletionRequest apiChatCompletionRequest);
    }
}
