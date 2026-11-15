using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.CreateImage
{
    public class CreateImageResponse
    {
        [JsonPropertyName("created")]
        public long Created { get; set; }
        [JsonPropertyName("data")]
        public List<CreateImageResponseData> Data { get; set; }
        [JsonPropertyName("usage")]
        public ModelUsage Usage { get; set; }
        [JsonPropertyName("size")]
        public string Size { get; set; }
        [JsonPropertyName("quality")]
        public string Quality { get; set; }
        [JsonPropertyName("output_format")]
        public string OutputFormat { get; set; }
        [JsonPropertyName("background")]
        public string Background { get; set; }
    }
}
