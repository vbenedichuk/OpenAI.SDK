using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Audio
{
    public class TranslationResponse
    {
        /// <summary>
        /// Transcription text
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
