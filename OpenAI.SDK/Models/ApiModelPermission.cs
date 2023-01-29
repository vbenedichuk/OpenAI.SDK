using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models
{
    public class ApiModelPermission
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("created")]
        public long Created { get; set; }
        [JsonPropertyName("allow_create_engine")]
        public bool AlowCreateEngine { get; set; }
        [JsonPropertyName("allow_sampling")]
        public bool AlowSampling { get; set; }
        [JsonPropertyName("allow_logprobs")]
        public bool AllowLogprobs { get; set; }
        [JsonPropertyName("allow_search_indices")]
        public bool AllowSearchIndices { get; set; }
        [JsonPropertyName("allow_view")]
        public bool AllowView {get;set;}
        [JsonPropertyName("allow_fine_tuning")]
        public bool AllowFineTuning { get; set; }
        [JsonPropertyName("organization")]
        public string Organization { get; set; }
        [JsonPropertyName("group")]
        public string Group { get; set; }
        [JsonPropertyName("is_blocking")]
        public bool IsBlocking { get; set; }
    }
}
