using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Edits
{
    /// <summary>
    /// Class to represent a response to an API request to create an edit.
    /// </summary>
    public class ApiCreateEditRespoinse
    {
        /// <summary>
        /// The type of the object that was created.
        /// </summary>
        [JsonPropertyName("object")]
        public string Object { get; set; }

        /// <summary>
        /// The time when the object was created, in Unix timestamp format.
        /// </summary>
        [JsonPropertyName("created")]
        public long Created { get; set; }
        /// <summary>
        /// The choices for the edit.
        /// </summary>
        [JsonPropertyName("choices")]
        public List<EditChoice> Choices { get; set; }
        /// <summary>
        /// The usage information for the model that was used to create the edit.
        /// </summary>
        [JsonPropertyName("usage")]
        public ModelUsage Usage { get; set; }
    }
}
