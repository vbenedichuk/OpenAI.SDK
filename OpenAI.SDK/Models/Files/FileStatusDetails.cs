using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Files
{
    /// <summary>
    /// Additional details about a file's processing status.
    /// </summary>
    public class FileStatusDetails
    {
        /// <summary>
        /// Machine-readable status code.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// Human-readable status message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
