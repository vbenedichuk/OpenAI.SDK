using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.Edits;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Open API api abstraction.
    /// </summary>
    public interface IOpenAiAPI
    {
        /// <summary>
        /// implements GET https://api.openai.com/v1/models
        /// Lists the currently available models, and provides basic information about each one such as the owner and availability.
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task<List<ApiModel>> GetModelsAsync();
        /// <summary>
        /// GET https://api.openai.com/v1/models/{model}
        /// Retrieves a model instance, providing basic information about the model such as the owner and permissioning.
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task<ApiModel> RetrieveModelAsync(string modelId);
        /// <summary>
        /// implements POST https://api.openai.com/v1/completions 
        /// Creates a completion for the provided prompt and parameters
        /// </summary>
        /// <param name="apiCompletionRequest"></param>
        /// <returns></returns>
        Task<ApiCompletionResponse> CreateCompletionAsync(ApiCompletionRequest apiCompletionRequest);
        /// <summary>
        /// implements POST https://api.openai.com/v1/edits
        /// Creates a new edit for the provided input, instruction, and parameters.
        /// </summary>
        /// <param name="createEditRequest"></param>
        /// <returns></returns>
        Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest);

    }
}
