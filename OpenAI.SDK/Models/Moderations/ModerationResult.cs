using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Moderations
{
    public class ModerationResult
    {
        [JsonPropertyName("categories")]
        public Dictionary<string, bool> Categories { get; set; }
        [JsonPropertyName("category_scores")]
        public Dictionary<string, double> CategoryScores { get; set; }
        [JsonPropertyName("flagged")]
        public bool Flagged { get; set; }
    }
}
