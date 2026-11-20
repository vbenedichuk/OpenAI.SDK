using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Edits;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    [Obsolete("API is not supported anymore")]
    internal class EditsApi : BaseApi, IEditsApi
    {
        public EditsApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        /// <inheritdoc/>
        public async Task<ApiCreateEditResponse> CreateEditAsync(ApiCreateEditRequest createEditRequest, CancellationToken cancellationToken = default)
        {
            if (createEditRequest == null)
            {
                throw new ArgumentNullException(nameof(createEditRequest));
            }
            if (string.IsNullOrWhiteSpace(createEditRequest.Model))
            {
                throw new ArgumentException("Model is required.", nameof(createEditRequest));
            }
            if (string.IsNullOrWhiteSpace(createEditRequest.Instruction))
            {
                throw new ArgumentException("Instruction is required.", nameof(createEditRequest));
            }
            var response = await ExecuteRequest<ApiCreateEditRequest, ApiCreateEditResponse>(HttpMethod.Post, "edits", createEditRequest, cancellationToken).ConfigureAwait(false);
            return response;
        }
    }
}
