using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Moderations
{
    public class ModerationRequest
    {
        [JsonPropertyName("input")]
        public List<string> Input { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
    }
}
