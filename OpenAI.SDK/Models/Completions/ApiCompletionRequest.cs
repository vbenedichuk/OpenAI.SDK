using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Completions
{
    public class ApiCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }
        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }
        [JsonPropertyName("temperature")]
        public int Temperatue { get; set; }
        [JsonPropertyName("top_p")]
        public int? TopP { get; set; }
        [JsonPropertyName("n")]
        public int? N { get; set; }
        [JsonPropertyName("stream")]
        public bool? Stream { get; set; }
        [JsonPropertyName("logprobe")]
        public int? Logprobe { get; set; }
        [JsonPropertyName("stop")]
        public string Stop { get; set; }
    }
}
