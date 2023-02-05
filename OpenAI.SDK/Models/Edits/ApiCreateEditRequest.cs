using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Edits
{
    public class ApiCreateEditRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("input")]
        public string Input { get; set; }
        [JsonPropertyName("instruction")]
        public string Instruction { get; set; }
        [JsonPropertyName("temperature")]
        public int Temperatue { get; set; }
        [JsonPropertyName("top_p")]
        public int? TopP { get; set; }
        [JsonPropertyName("n")]
        public int? N { get; set; }
    }
}
