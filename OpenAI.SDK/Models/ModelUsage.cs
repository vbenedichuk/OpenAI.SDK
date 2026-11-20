using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models
{
    public class ModelUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
        [JsonPropertyName("prompt_tokens_details")]
        public PromptTokensDetails PromptTokensDetails { get; set; }
        [JsonPropertyName("completion_tokens_details")]
        public CompletionTokensDetails CompletionTokensDetails { get; set; }
    }

    public class PromptTokensDetails
    {
        [JsonPropertyName("cached_tokens")]
        public int? CachedTokens { get; set; }
        [JsonPropertyName("audio_tokens")]
        public int? AudioTokens { get; set; }
    }

    public class CompletionTokensDetails
    {
        [JsonPropertyName("reasoning_tokens")]
        public int? ReasoningTokens { get; set; }
        [JsonPropertyName("audio_tokens")]
        public int? AudioTokens { get; set; }
        [JsonPropertyName("accepted_prediction_tokens")]
        public int? AcceptedPredictionTokens { get; set; }
        [JsonPropertyName("rejected_prediction_tokens")]
        public int? RejectedPredictionTokens { get; set; }
    }
}
