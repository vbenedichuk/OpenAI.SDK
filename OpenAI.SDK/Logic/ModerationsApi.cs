using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models.Moderations;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task<ModerationResponse> CreateModerationAsync(string input, string model = null, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input must not be empty.", nameof(input));
            }
            return CreateModerationAsync(new string[] { input }, model, cancellationToken);
        }

        public async Task<ModerationResponse> CreateModerationAsync(string[] input, string model = null, CancellationToken cancellationToken = default)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            if (input.Length == 0 || input.All(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Input must contain at least one non-empty entry.", nameof(input));
            }
            var result = await ExecuteRequest<ModerationRequest, ModerationResponse>(HttpMethod.Post, "moderations",
                new ModerationRequest { Input = new List<string>(input), Model = model }, cancellationToken).ConfigureAwait(false);
            return result;
        }
    }
}
