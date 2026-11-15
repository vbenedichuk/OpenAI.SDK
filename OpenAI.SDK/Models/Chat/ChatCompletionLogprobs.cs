using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatCompletionLogprobs
    {
        [JsonPropertyName("content")]
        public object Content { get; set; }

        [JsonPropertyName("refusal")]
        public object Refusal { get; set; }
    }
}
