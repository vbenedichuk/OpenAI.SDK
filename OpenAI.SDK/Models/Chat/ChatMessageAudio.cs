using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Audio content returned alongside an assistant message.
    /// </summary>
    public class ChatMessageAudio
    {
        /// <summary>
        /// Unique identifier of the audio asset.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Unix timestamp (in seconds) when the audio asset expires.
        /// </summary>
        [JsonPropertyName("expires_at")]
        public long ExpiresAt { get; set; }

        /// <summary>
        /// Base64-encoded audio data.
        /// </summary>
        [JsonPropertyName("data")]
        public string Data { get; set; }

        /// <summary>
        /// Transcript of the audio.
        /// </summary>
        [JsonPropertyName("transcript")]
        public string Transcript { get; set; }
    }
}
