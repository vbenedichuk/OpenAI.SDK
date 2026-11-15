using OpenAI.SDK.Models.Chat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IChatApi
    {
        /// <summary>
        /// POST https://api.openai.com/v1/chat/completions 
        /// Creates a completion for the chat message
        /// </summary>
        /// <param name="apiChatCompletionRequest"></param>
        /// <returns></returns>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<ApiChatCompletionResponse> CreateCompletionAsync(ApiChatCompletionRequest apiChatCompletionRequest);

        /// <summary>
        /// POST https://api.openai.com/v1/chat/completions 
        /// Creates a completion for the chat message
        /// </summary>
        /// <param name="apiChatCompletionRequest"></param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        Task<ApiChatCompletionResponse> CreateCompletionAsync(ApiChatCompletionRequest apiChatCompletionRequest, CancellationToken cancellationToken = default);
    }
}
