using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models.CreateImage;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ImagesApi : BaseApi, IImagesApi
    {
        public ImagesApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request)
        {
            return CreateImageAsync(request, CancellationToken.None);
        }

        public async Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<CreateImageRequest, CreateImageResponse>(HttpMethod.Post, "images/generations", request, cancellationToken);
            return response;
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<CreateImageResponse> CreateImageEditAsync(Stream image, string imageFileName, string prompt, Stream mask = null,
            string maskFileName = null, int? n = null, string size = null, string responseFormat = null, string user = null)
        {
            return CreateImageEditAsync(image, imageFileName, prompt, mask, maskFileName, n, size, responseFormat, user, CancellationToken.None);
        }

        public async Task<CreateImageResponse> CreateImageEditAsync(Stream image, string imageFileName, string prompt, Stream mask = null,
            string maskFileName = null, int? n = null, string size = null, string responseFormat = null, string user = null,
            CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, "images/edits");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var content = PrepareRequestContent(image, imageFileName, prompt, mask, maskFileName, n, size, responseFormat, user);
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<CreateImageResponse>(stream, cancellationToken: cancellationToken);
                }
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<CreateImageResponse> CreateImageVariation(Stream image, string imageFileName, int? n = null, string size = null,
            string responseFormat = null, string user = null)
        {
            return CreateImageVariation(image, imageFileName, n, size, responseFormat, user, CancellationToken.None);
        }

        public async Task<CreateImageResponse> CreateImageVariation(Stream image, string imageFileName, int? n = null, string size = null,
            string responseFormat = null, string user = null, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, "images/variations");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var content = PrepareRequestContent(image, imageFileName, null, null, null, n, size, responseFormat, user);
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<CreateImageResponse>(stream, cancellationToken: cancellationToken);
                }
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }

        private static MultipartFormDataContent PrepareRequestContent(Stream image, string imageFileName, string prompt, Stream mask = null,
            string maskFileName = null, int? n = null, string size = null, string responseFormat = null, string user = null)
        {
            var content = new MultipartFormDataContent()
            {
                { new StreamContent(image), "image", imageFileName }
            };
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                content.Add(new StringContent(prompt), "prompt");
            }
            if (mask != null)
            {
                content.Add(new StreamContent(mask), "mask", maskFileName);
            }
            if (n != null)
            {
                content.Add(new StringContent(n.ToString()), "n");
            }
            if (!string.IsNullOrWhiteSpace(size))
            {
                content.Add(new StringContent(size), "size");
            }
            if (!string.IsNullOrWhiteSpace(responseFormat))
            {
                content.Add(new StringContent(responseFormat), "response_format");
            }
            if (user != null)
            {
                content.Add(new StringContent(user), "user");
            }
            return content;
        }
    }
}
