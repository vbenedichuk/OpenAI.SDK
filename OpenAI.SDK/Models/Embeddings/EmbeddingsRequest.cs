using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Embeddings
{
    public class EmbeddingsRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("input")]
        public string[] Input { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }
}
