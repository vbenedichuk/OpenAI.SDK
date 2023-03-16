using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// Chat message format.
    /// See <see href="https://platform.openai.com/docs/guides/chat/introduction">the documentation</see> for more details
    /// </summary>
    public class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
