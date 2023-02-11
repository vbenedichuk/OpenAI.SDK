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
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ImagesApi : BaseApi, IImagesApi
    {
        public ImagesApi(IHttpClientFactory clientFactory,
            IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        public async Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request)
        {
            var response = await ExecuteRequest<CreateImageResponse>(HttpMethod.Post, "images/generations");
            return response;          
        }

        public async Task<CreateImageResponse> CreateImageEditAsync(Stream image, string prompt, Stream mask = null, 
            int? n = null, string size = null, string responseFormat = null, string user = null)
        {
            var url = new Uri(_baseUri, "images/edits");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var content = PrepareRequestContent(image, prompt, mask, n, size, responseFormat, user);
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CreateImageResponse>(contentString);
                return result;
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }

        public async Task<CreateImageResponse> CreateImageVariation(Stream image, int? n = null, string size = null, string responseFormat = null, string user = null)
        {
            var url = new Uri(_baseUri, "images/variations");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var content = PrepareRequestContent(image, null, null, n, size, responseFormat, user);
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CreateImageResponse>(contentString);
                return result;
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }

        private MultipartFormDataContent PrepareRequestContent(Stream image, string prompt, Stream mask = null,
            int? n = null, string size = null, string responseFormat = null, string user = null)
        {

            var content = new MultipartFormDataContent
            {
                {new StreamContent(image), "image", "image.png" }
            };
            content.Add(new StringContent(prompt), "prompt");
            if (mask != null)
            {
                content.Add(new StreamContent(mask), "mask", "mask.png");
            }
            if (n != null)
            {
                content.Add(new StringContent(n.ToString()), "n");
            }
            if (size != null)
            {
                content.Add(new StringContent(size.ToString()), "size");
            }
            if (responseFormat != null)
            {
                content.Add(new StringContent(responseFormat.ToString()), "response_format");
            }
            if (user != null)
            {
                content.Add(new StringContent(user), "user");
            }
            return content;
        }
    }
}
