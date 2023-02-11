using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.CreateImage
{
    public class CreateImageRequest
    {
        /// <summary>
        /// The text description of the desired image(s).
        /// Maximum length is 1000 characters.
        /// </summary>
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; }
        /// <summary>
        /// The number of images to generate.
        /// Must be between 1 and 10.
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; }
        /// <summary>
        /// The size of the generated images.
        /// Must be one of 256x256, 512x512, or 1024x1024.
        /// </summary>
        [JsonPropertyName("size")]
        public string Size { get; set; }
        /// <summary>
        /// The format in which the generated images are returned.
        /// Must be one of "url" or "b64_json".
        /// </summary>
        [JsonPropertyName("response_format")]
        public string ResponseFormat { get; set; }
        /// <summary>
        /// A unique identifier representing the end-user, which can help OpenAI to monitor and detect abuse.
        /// <see href="https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids">Learn more</see>.
        /// </summary>
        [JsonPropertyName("user")]
        public string User { get; set; }
    }
}
