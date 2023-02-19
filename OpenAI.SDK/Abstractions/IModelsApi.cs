using OpenAI.SDK.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// List and describe the various models available in the API. You can refer to the <see href="https://platform.openai.com/docs/models">Models</see> documentation to understand what models are available and the differences between them.
    /// </summary>
    public interface IModelsApi
    {
        /// <summary>
        /// implements GET https://api.openai.com/v1/models
        /// Lists the currently available models, and provides basic information about each one such as the owner and availability.
        /// </summary>
        /// <param name="modelId"></param>
        /// <returns></returns>
        Task<List<ApiModel>> GetModelsAsync();
        /// <summary>
        /// GET https://api.openai.com/v1/models/{modelId}
        /// Retrieves a model instance, providing basic information about the model such as the owner and permissioning.
        /// </summary>
        /// <param name="modelId">The ID of the model to use for this request</param>
        /// <returns></returns>
        Task<ApiModel> RetrieveModelAsync(string modelId);
    }
}
