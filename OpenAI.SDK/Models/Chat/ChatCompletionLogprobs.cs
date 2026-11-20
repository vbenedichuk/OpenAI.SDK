using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Log probability information returned when <c>logprobs</c> is requested.
    /// </summary>
    public class ChatCompletionLogprobs
    {
        /// <summary>
        /// Log probability information for the generated content.
        /// </summary>
        [JsonPropertyName("content")]
        public List<ChatTokenLogprob> Content { get; set; }

        /// <summary>
        /// Log probability information for a refusal.
        /// </summary>
        [JsonPropertyName("refusal")]
        public List<ChatTokenLogprob> Refusal { get; set; }
    }
}
