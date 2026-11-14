using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Moderations;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ModerationsApi : BaseApi, IModerationsApi
    {
        public ModerationsApi(IHttpClientFactory clientFactory, IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<ModerationResponse> CreateModeration(string input, string model = null)
        {
            return CreateModerationAsync(input, model, CancellationToken.None);
        }

        public Task<ModerationResponse> CreateModerationAsync(string input, string model = null, CancellationToken cancellationToken = default)
        {
            return CreateModerationAsync(new string[] { input }, model, cancellationToken);
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<ModerationResponse> CreateModeration(string[] input, string model = null)
        {
            return CreateModerationAsync(input, model, CancellationToken.None);
        }

        public async Task<ModerationResponse> CreateModerationAsync(string[] input, string model = null, CancellationToken cancellationToken = default)
        {
            var result = await ExecuteRequest<ModerationRequest, ModerationResponse>(HttpMethod.Post, "moderations",
                new ModerationRequest { Input = new List<string>(input), Model = model }, cancellationToken);
            return result;
        }
    }
}
