using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Completions
{
    /// <summary>
    /// Log probability information returned when <c>logprobs</c> is requested.
    /// </summary>
    public class CompletionLogprobs
    {
        /// <summary>
        /// The tokens for which log probabilities were returned.
        /// </summary>
        [JsonPropertyName("tokens")]
        public List<string> Tokens { get; set; }

        /// <summary>
        /// Log probabilities of the sampled tokens. Values can be null when the token was not sampled.
        /// </summary>
        [JsonPropertyName("token_logprobs")]
        public List<double?> TokenLogprobs { get; set; }

        /// <summary>
        /// The most likely tokens and their log probabilities at each position.
        /// </summary>
        [JsonPropertyName("top_logprobs")]
        public List<Dictionary<string, double>> TopLogprobs { get; set; }

        /// <summary>
        /// The character offsets of the tokens in the original text.
        /// </summary>
        [JsonPropertyName("text_offset")]
        public List<int> TextOffset { get; set; }
    }
}
