using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatToolCallFunction
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; }
    }
}