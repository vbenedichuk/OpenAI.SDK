using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Moderations;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ModerationsApi : BaseApi, IModerationsApi
    {
        public ModerationsApi(IHttpClientFactory clientFactory, IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        public Task<ModerationResponse> CreateModeration(string input, string model = null)
        {
            return CreateModeration(new string[] { input }, model);
        }

        public async Task<ModerationResponse> CreateModeration(string[] input, string model = null)
        {
            var result = await ExecuteRequest<ModerationRequest, ModerationResponse>(HttpMethod.Post, "moderations", 
                new ModerationRequest { Input = new List<string>(input), Model = model });
            return result;
        }
    }
}
