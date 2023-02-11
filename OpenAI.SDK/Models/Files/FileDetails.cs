using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Files
{
    public class FileDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("bytes")]
        public long Bytes { get; set; }
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }
        [JsonPropertyName("filename")]
        public string Filename { get; set; }
        [JsonPropertyName("purpose")]
        public string Purpose { get; set; }
    }
}
