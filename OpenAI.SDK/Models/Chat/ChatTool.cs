using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatTool
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        [JsonPropertyName("function")]
        public ChatToolFunction Function { get; set; }
    }
}