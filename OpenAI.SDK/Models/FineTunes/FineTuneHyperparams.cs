using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.FineTunes
{
    public class FineTuneHyperparams
    {
        [JsonPropertyName("batch_size")]
        public int BatchSize { get; set; }
        [JsonPropertyName("learning_rate_multiplier")]
        public double LearningRateMultiplier { get; set; }
        [JsonPropertyName("n_epochs")]
        public int NEpochs { get; set; }
        [JsonPropertyName("prompt_loss_weight")]
        public double PromptLossWeitghs { get; set; }
    }
}
