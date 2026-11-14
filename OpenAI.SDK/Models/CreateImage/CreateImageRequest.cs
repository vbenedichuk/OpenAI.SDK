using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.CreateImage
{
    public class CreateImageRequest
    {
        /// <summary>
        /// The text description of the desired image(s).
        /// </summary>
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }

        /// <summary>
        /// The image generation model to use (for example, gpt-image-1 or dall-e-2).
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }

        /// <summary>
        /// The number of images to generate.
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; }

        /// <summary>
        /// The size of the generated images.
        /// </summary>
        [JsonPropertyName("size")]
        public string Size { get; set; }

        /// <summary>
        /// The quality of the generated image (model-specific, for example low, medium, high, standard, hd).
        /// </summary>
        [JsonPropertyName("quality")]
        public string Quality { get; set; }

        /// <summary>
        /// The format in which the generated images are returned (legacy models: url or b64_json).
        /// </summary>
        [JsonPropertyName("response_format")]
        public string ResponseFormat { get; set; }

        /// <summary>
        /// The output format for generated images (for example png, jpeg, webp).
        /// </summary>
        [JsonPropertyName("output_format")]
        public string OutputFormat { get; set; }

        /// <summary>
        /// The background setting for generated images (for example transparent, opaque, auto).
        /// </summary>
        [JsonPropertyName("background")]
        public string Background { get; set; }

        /// <summary>
        /// A unique identifier representing the end-user, which can help OpenAI to monitor and detect abuse.
        /// <see href="https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids">Learn more</see>.
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; }
    }
}
