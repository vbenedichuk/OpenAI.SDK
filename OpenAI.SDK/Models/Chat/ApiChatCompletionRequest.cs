using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Request body for the Chat Completions endpoint.
    /// </summary>
    public class ApiChatCompletionRequest
    {
        /// <summary>
        /// Id of the model to use, for example <c>gpt-4o-mini</c>.
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// Conversation messages, oldest first.
        /// </summary>
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; }

        /// <summary>
        /// Sampling temperature between 0 and 2. Higher values make the output more random.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperature { get; set; }

        /// <summary>
        /// Nucleus sampling probability mass. Alternative to <see cref="Temperature"/>.
        /// </summary>
        [JsonPropertyName("top_p")]
        public double? TopP { get; set; }

        /// <summary>
        /// How many chat completion choices to generate for each input message.
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; }

        /// <summary>
        /// Streaming is not supported by this SDK: setting this to <c>true</c> throws
        /// <see cref="System.NotSupportedException"/>.
        /// </summary>
        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }

        /// <summary>
        /// Up to 4 sequences where the API stops generating. Accepts a string or an array of strings.
        /// </summary>
        [JsonPropertyName("stop")]
        public object Stop { get; set; }

        /// <summary>
        /// Maximum number of tokens to generate. Ignored by reasoning models, which require
        /// <see cref="MaxCompletionTokens"/> instead.
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Upper bound on generated tokens, including reasoning tokens. Preferred over
        /// <see cref="MaxTokens"/> for current models.
        /// </summary>
        [JsonPropertyName("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }

        /// <summary>
        /// Penalizes tokens that already appeared in the text, from -2 to 2.
        /// </summary>
        [JsonPropertyName("presence_penalty")]
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// Penalizes tokens by their frequency in the text so far, from -2 to 2.
        /// </summary>
        [JsonPropertyName("frequency_penalty")]
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Per-token bias, from -100 to 100, keyed by token id.
        /// </summary>
        [JsonPropertyName("logit_bias")]
        public Dictionary<string, float> LogitBias { get; set; }

        /// <summary>
        /// Stable identifier for the end user, used for abuse monitoring.
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; }

        /// <summary>
        /// Output format constraint, for example <c>{"type":"json_object"}</c> or a JSON schema.
        /// </summary>
        [JsonPropertyName("response_format")]
        public ChatResponseFormat ResponseFormat { get; set; }

        /// <summary>
        /// Best-effort deterministic sampling seed.
        /// </summary>
        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// Tools the model may call.
        /// </summary>
        [JsonPropertyName("tools")]
        public List<ChatTool> Tools { get; set; }

        /// <summary>
        /// Controls which tool is called: <c>"none"</c>, <c>"auto"</c>, <c>"required"</c>,
        /// or an object such as <c>{"type":"function","function":{"name":"get_weather"}}</c>.
        /// </summary>
        [JsonPropertyName("tool_choice")]
        public object ToolChoice { get; set; }

        /// <summary>
        /// Legacy function definitions (deprecated in favour of <see cref="Tools"/>).
        /// </summary>
        [JsonPropertyName("functions")]
        public List<ChatToolFunction> Functions { get; set; }

        /// <summary>
        /// Legacy function selection: <c>"none"</c>, <c>"auto"</c> or an object naming the function.
        /// </summary>
        [JsonPropertyName("function_call")]
        public object FunctionCall { get; set; }

        /// <summary>
        /// Whether to return log probabilities of the output tokens.
        /// </summary>
        [JsonPropertyName("logprobs")]
        public bool? Logprobs { get; set; }

        /// <summary>
        /// Number of most likely tokens to return at each position. Requires <see cref="Logprobs"/>.
        /// </summary>
        [JsonPropertyName("top_logprobs")]
        public int? TopLogprobs { get; set; }
    }
}
