using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Edits;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class EditsApi : BaseApi, IEditsApi
    {
        public EditsApi(IHttpClientFactory clientFactory, IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        /// </<inheritdoc/>
        public async Task<ApiCreateEditRespoinse> CreateEdit(ApiCreateEditRequest createEditRequest)
        {
            var response = await ExecuteRequest<ApiCreateEditRequest, ApiCreateEditRespoinse>(HttpMethod.Post, "edits", createEditRequest);
            return response;
        }
    }
}
