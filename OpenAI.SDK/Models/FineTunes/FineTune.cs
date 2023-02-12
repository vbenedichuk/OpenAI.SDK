using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.FineTunes
{
    public class FineTune
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("object")]
        public string Object { get; set; }
        [JsonPropertyName("model")]
        public string Model { get; set; }
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }
        [JsonPropertyName("events")]
        public List<FineTuneEvent> Events { get; set; }
        [JsonPropertyName("fine_tuned_model")]
        public string FimeTunedModel { get; set; }
        [JsonPropertyName("hyperparams")]
        public FineTuneHyperparams Hyperparams { get; set; }
        [JsonPropertyName("organization_id")]
        public string OrganizationId { get; set; }
        [JsonPropertyName("result_files")]
        public List<FileInfo> ResultFiles { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("validation_files")]
        public List<FileInfo> ValidationFiles { get; set; }
        [JsonPropertyName("training_files")]
        public List<FileInfo> TrainingFiles { get; set; }
        [JsonPropertyName("updated_at")]
        public long UpdatedAt { get; set; }
    }
}
