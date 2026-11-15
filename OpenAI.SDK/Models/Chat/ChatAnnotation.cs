using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Chat
{
    /// <summary>
    /// An annotation attached to a chat message, for example a web search citation.
    /// </summary>
    public class ChatAnnotation
    {
        /// <summary>
        /// The annotation type, for example <c>url_citation</c>.
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// Citation details, present when <see cref="Type"/> is <c>url_citation</c>.
        /// </summary>
        [JsonPropertyName("url_citation")]
        public ChatUrlCitation UrlCitation { get; set; }
    }

    /// <summary>
    /// A URL citation attached to a chat message.
    /// </summary>
    public class ChatUrlCitation
    {
        /// <summary>
        /// Start index of the cited text in the message content.
        /// </summary>
        [JsonPropertyName("start_index")]
        public int StartIndex { get; set; }

        /// <summary>
        /// End index of the cited text in the message content.
        /// </summary>
        [JsonPropertyName("end_index")]
        public int EndIndex { get; set; }

        /// <summary>
        /// Title of the cited page.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// URL of the cited page.
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
