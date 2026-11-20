using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ChatFunctionCall
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("arguments")]
        public string Arguments { get; set; }
    }
}