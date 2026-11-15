using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Chat;
using System;
using System.Net.Http;
using System.Threading;
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
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<ApiChatCompletionResponse> CreateCompletionAsync(ApiChatCompletionRequest apiChatCompletionRequest)
        {
            return CreateCompletionAsync(apiChatCompletionRequest, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<ApiChatCompletionResponse> CreateCompletionAsync(ApiChatCompletionRequest apiChatCompletionRequest, CancellationToken cancellationToken = default)
        {
            if (apiChatCompletionRequest == null)
            {
                throw new ArgumentNullException(nameof(apiChatCompletionRequest));
            }
            if (string.IsNullOrWhiteSpace(apiChatCompletionRequest.Model))
            {
                throw new ArgumentException("Model is required.", nameof(apiChatCompletionRequest));
            }
            if (apiChatCompletionRequest.Messages == null || apiChatCompletionRequest.Messages.Count == 0)
            {
                throw new ArgumentException("Messages are required.", nameof(apiChatCompletionRequest));
            }
            if (apiChatCompletionRequest.Stream == true)
            {
                throw new NotSupportedException("Streaming responses are not supported by this SDK.");
            }

            var response = await ExecuteRequest<ApiChatCompletionRequest, ApiChatCompletionResponse>(HttpMethod.Post, "chat/completions", apiChatCompletionRequest, cancellationToken).ConfigureAwait(false);
            return response;
        }
    }
}