using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Files
{
    public class ListFilesResponse
    {
        /// <summary>
        /// Files list
        /// </summary>
        [JsonPropertyName("data")]
        public List<FileDetails> Data { get; set; }
        /// <summary>
        /// Object type. (list)
        /// </summary>
        [JsonPropertyName("object")]
        public string Object { get; set; }
    }
}
