using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ApiChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }

        [JsonPropertyName("n")]
        public int? N { get; set; }

        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        [JsonPropertyName("stop")]
        public object Stop { get; set; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        [JsonPropertyName("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }

        [JsonPropertyName("presence_penalty")]
        public double? PresencePenalty { get; set; }

        [JsonPropertyName("frequency_penalty")]
        public double? FrequencyPenalty { get; set; }

        [JsonPropertyName("logit_bias")]
        public Dictionary<string, float> LogitBias { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("response_format")]
        public ChatResponseFormat ResponseFormat { get; set; }

        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        [JsonPropertyName("tools")]
        public List<ChatTool> Tools { get; set; }

        [JsonPropertyName("tool_choice")]
        public object ToolChoice { get; set; }

        [JsonPropertyName("functions")]
        public List<ChatToolFunction> Functions { get; set; }

        [JsonPropertyName("function_call")]
        public object FunctionCall { get; set; }

        [JsonPropertyName("logprobs")]
        public bool? Logprobs { get; set; }

        [JsonPropertyName("top_logprobs")]
        public int? TopLogprobs { get; set; }
    }
}