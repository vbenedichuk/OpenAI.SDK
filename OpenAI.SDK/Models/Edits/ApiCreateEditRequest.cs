using System.Text.Json.Serialization;

namespace OpenAI.SDK.Models.Edits
{
    public class ApiCreateEditRequest
    {
        /// <summary>
        /// The ID of the model to use.
        /// Can be either "text-davinci-edit-001" or "code-davinci-edit-001".
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; }
        /// <summary>
        /// The input text to use as a starting point for the edit.
        /// </summary>
        [JsonPropertyName("input")]
        public string Input { get; set; }
        /// <summary>
        /// The instruction that tells the model how to edit the input.
        /// </summary>
        [JsonPropertyName("instruction")]
        public string Instruction { get; set; }
        /// <summary>
        /// The sampling temperature to use, between 0 and 2.
        /// Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
        /// </summary>
        [JsonPropertyName("temperature")]
        public float Temperatue { get; set; }
        /// <summary>
        /// The top_p value to use for nucleus sampling, where the model considers the results of the tokens with top_p probability mass.
        /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
        /// </summary>
        [JsonPropertyName("top_p")]
        public int? TopP { get; set; }
        /// <summary>
        /// The number of edits to generate for the input and instruction.
        /// </summary>
        [JsonPropertyName("n")]
        public int? N { get; set; }
    }
}
