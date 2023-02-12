using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.FineTunes
{
    public class FineTuneEvent
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }
        [JsonPropertyName("level")]
        public string Level { get; set; }
        [JsonPropertyName("messge")]
        public string Message { get; set; }
    }
}
