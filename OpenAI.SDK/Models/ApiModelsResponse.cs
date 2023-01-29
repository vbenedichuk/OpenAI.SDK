using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models
{
    public class ApiModelsResponse
    {
        [JsonPropertyName("data")]
        public List<ApiModel> Data { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
    }
}
