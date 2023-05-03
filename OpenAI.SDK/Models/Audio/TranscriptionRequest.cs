using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Audio
{
    public class TranscriptionRequest
    {
        public string File { get; set; }
        public string Model { get; set; }
        public string Prompt { get; set; }
        public string ResponseFormat { get; set; }
        /// <summary>
        /// What sampling temperature to use, between 0 and 2. Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
        /// We generally recommend altering this or top_p but not both.
        /// </summary>
        [JsonPropertyName("temperature")]
        public double? Temperatue { get; set; }

        public string Language { get; set; }
    }
}
