using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    public class ApiChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("object")]
        public string Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("system_fingerprint")]
        public string SystemFingerprint { get; set; }

        [JsonPropertyName("service_tier")]
        public string ServiceTier { get; set; }

        [JsonPropertyName("choices")]
        public List<ChatCompletionChoice> Choices { get; set; }

        [JsonPropertyName("usage")]
        public ModelUsage Usage { get; set; }
    }
}