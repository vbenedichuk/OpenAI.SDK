using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Embeddings
{
    public class EmbeddingsRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        /// <summary>
        /// Input text to embed, encoded as a string or array of strings/tokens.
        /// </summary>
        [JsonPropertyName("input")]
        public object Input { get; set; }
        [JsonPropertyName("user")]
        public string User { get; set; }
    }
}
