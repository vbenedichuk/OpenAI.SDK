using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Engines
{
    public class EngineInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("owner")]
        public string Owner { get; set; }
        [JsonPropertyName("ready")]
        public bool Ready { get; set; }
    }
}
