using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Edits
{
    /// <summary>
    /// Class to represent a choice for an edit in a text.
    /// </summary>
    public class EditChoice
    {
        /// <summary>
        /// The text of the choice.
        /// </summary>
        [JsonPropertyName("text")]
        public string Text { get; set; }
        /// <summary>
        /// The index of the choice.
        /// </summary>
        [JsonPropertyName("index")]
        public int Index { get; set; }
    }
}
