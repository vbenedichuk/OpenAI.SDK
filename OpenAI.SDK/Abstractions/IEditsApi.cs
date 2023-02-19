using OpenAI.SDK.Models.Edits;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IEditsApi
    {
        /// <summary>
        /// implements POST https://api.openai.com/v1/edits
        /// Creates a new edit for the provided input, instruction, and parameters.
        /// </summary>
        /// <param name="createEditRequest"></param>
        /// <returns></returns>
        Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest);
    }
}
