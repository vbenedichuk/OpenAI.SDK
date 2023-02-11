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
    }
}
