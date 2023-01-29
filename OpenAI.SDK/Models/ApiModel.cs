﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models
{
    public class ApiModel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("owned_by")]
        public string OwnedBy { get; set; }
        [JsonPropertyName("permission")]
        public List<ApiModelPermission> Permission { get; set; }
    }
}
