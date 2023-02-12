using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Moderations
{
    public class ModerationResult
    {
        [JsonPropertyName("categories")]
        Dictionary<string, bool> Categories { get; set; }
        [JsonPropertyName("category_scores")]
        Dictionary<string, double> CategoryScores { get; set; }
        [JsonPropertyName("flagged")]
        bool Flagged { get; set; }
    }
}
