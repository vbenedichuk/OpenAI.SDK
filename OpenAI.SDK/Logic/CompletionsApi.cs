using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.Edits;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{

    internal class CompletionsApi : BaseApi, ICompletionsApi
    {
        public CompletionsApi(IHttpClientFactory clientFactory,
            IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        /// </<inheritdoc/>
        public async Task<ApiCompletionResponse> CreateCompletionAsync(ApiCompletionRequest apiCompletionRequest)
        {
            var response = await ExecuteRequest<ApiCompletionRequest, ApiCompletionResponse>(HttpMethod.Post, "completions", apiCompletionRequest);
            return response;
        }        
    }
}
