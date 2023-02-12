using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models
{
    public class ListResponse<T>
    {
        /// <summary>
        /// Objects list
        /// </summary>
        [JsonPropertyName("data")]
        public List<T> Data { get; set; }
        /// <summary>
        /// Object type. (list)
        /// </summary>
        [JsonPropertyName("object")]
        public string Object { get; set; }
    }
}
