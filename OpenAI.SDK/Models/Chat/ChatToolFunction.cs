using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatToolFunction
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("parameters")]
        public object Parameters { get; set; }
    }
}