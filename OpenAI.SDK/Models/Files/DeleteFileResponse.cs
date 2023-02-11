using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Files
{
    public class DeleteFileResponse
    {

        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("deleted")]
        public bool Deleted { get; set; }
    }
}
