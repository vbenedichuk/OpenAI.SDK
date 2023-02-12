using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.FineTunes
{
    public class DeleteFineTuneResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }
    }
}
