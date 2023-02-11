using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Embeddings
{
    public class EmbeddingData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("embedding")]
        public double[] Embedding { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
    }
}
