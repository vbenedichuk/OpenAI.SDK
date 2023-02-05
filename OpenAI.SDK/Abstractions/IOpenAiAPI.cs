using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.Edits;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Open API api abstraction.
    /// </summary>
    public interface IOpenAiAPI
    {
        Task<List<ApiModel>> GetModelsAsync();
        Task<ApiModel> GetModelAsync(string modelId);
        Task<ApiCompletionResponse> CreateCompletionAsync(ApiCompletionRequest apiCompletionRequest);
        Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest);

    }
}
