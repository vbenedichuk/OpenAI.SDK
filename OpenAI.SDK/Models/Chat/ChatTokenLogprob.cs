using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Log probability information for a generated token.
    /// </summary>
    public class ChatTokenLogprob
    {
        /// <summary>
        /// The token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// The log probability of the token.
        /// </summary>
        [JsonPropertyName("logprob")]
        public double Logprob { get; set; }

        /// <summary>
        /// The UTF-8 bytes of the token, when available.
        /// </summary>
        [JsonPropertyName("bytes")]
        public List<int> Bytes { get; set; }

        /// <summary>
        /// The most likely tokens and their log probabilities at this position.
        /// </summary>
        [JsonPropertyName("top_logprobs")]
        public List<ChatTopLogprob> TopLogprobs { get; set; }
    }

    /// <summary>
    /// A candidate token and its log probability in a logprobs list.
    /// </summary>
    public class ChatTopLogprob
    {
        /// <summary>
        /// The token.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// The log probability of the token.
        /// </summary>
        [JsonPropertyName("logprob")]
        public double Logprob { get; set; }

        /// <summary>
        /// The UTF-8 bytes of the token, when available.
        /// </summary>
        [JsonPropertyName("bytes")]
        public List<int> Bytes { get; set; }
    }
}
