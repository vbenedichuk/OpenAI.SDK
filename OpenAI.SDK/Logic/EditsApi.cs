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
    internal class EditsApi : BaseApi, IEditsApi
    {
        public EditsApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest)
        {
            return CreateEdit(createEditRequest, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ApiCreateEditRequest, ApiCreateEditRespoinse>(HttpMethod.Post, "edits", createEditRequest, cancellationToken);
            return response;
        }
    }
}
