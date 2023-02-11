using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.CreateImage
{
    public class CreateImageResponseData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
        [JsonPropertyName("b64_json")]
        public string B64Json { get; set; }
    }
}
