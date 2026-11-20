using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.CreateImage
{
    /// <summary>
    /// Token usage for an image generation or edit request.
    /// </summary>
    public class ImageUsage
    {
        /// <summary>
        /// Total number of tokens used.
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        /// <summary>
        /// Number of input tokens.
        /// </summary>
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        /// <summary>
        /// Number of output tokens.
        /// </summary>
        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }

        /// <summary>
        /// Breakdown of the input tokens.
        /// </summary>
        [JsonPropertyName("input_tokens_details")]
        public ImageInputTokensDetails InputTokensDetails { get; set; }
    }

    /// <summary>
    /// Breakdown of image input tokens.
    /// </summary>
    public class ImageInputTokensDetails
    {
        /// <summary>
        /// Number of text tokens.
        /// </summary>
        [JsonPropertyName("text_tokens")]
        public int TextTokens { get; set; }

        /// <summary>
        /// Number of image tokens.
        /// </summary>
        [JsonPropertyName("image_tokens")]
        public int ImageTokens { get; set; }
    }
}
