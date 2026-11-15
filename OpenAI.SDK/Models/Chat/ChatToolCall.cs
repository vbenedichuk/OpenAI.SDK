using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatToolCall
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public ChatToolCallFunction Function { get; set; }
    }
}