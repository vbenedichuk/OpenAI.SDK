using OpenAI.SDK.Models.Edits;
using System;
using System.Threading;
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
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<ApiCreateEditResponse> CreateEdit(ApiCreateEditRequest createEditRequest);

        /// <summary>
        /// implements POST https://api.openai.com/v1/edits
        /// Creates a new edit for the provided input, instruction, and parameters.
        /// </summary>
        /// <param name="createEditRequest"></param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        Task<ApiCreateEditResponse> CreateEditAsync(ApiCreateEditRequest createEditRequest, CancellationToken cancellationToken = default);
    }
}
