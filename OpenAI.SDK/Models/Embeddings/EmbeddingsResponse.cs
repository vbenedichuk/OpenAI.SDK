using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Embeddings
{
    public class EmbeddingsResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("usage")]
        public ModelUsage Usage { get; set; }
    }
}
