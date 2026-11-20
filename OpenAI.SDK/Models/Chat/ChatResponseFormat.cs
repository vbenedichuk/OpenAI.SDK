using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatResponseFormat
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("json_schema")]
        public object JsonSchema { get; set; }
    }
}