using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models.Audio;
using OpenAI.SDK.Models.CreateImage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OpenAI.SDK.Configuration;

namespace OpenAI.SDK.Logic
{
    internal class AudioApi : BaseApi, IAudio
    {
        public AudioApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }
        public async Task<TranscriptionResponse> CreateTranscription(Stream file, string fileName, string model, string prompt, 
            string responseFormat, double? temperatue, string language)
        {
            var url = new Uri(_baseUri, "audio/transcriptions");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var content = new MultipartFormDataContent()
            {
                { new StreamContent(file), "file", fileName }
            };
            if (!string.IsNullOrWhiteSpace(model))
            {
                content.Add(new StringContent(model), "model");
            }
            else
            {
                throw new ArgumentException("Model name can not be null", nameof(model));
            }
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                content.Add(new StringContent(prompt), "prompt");
            }
            if (!string.IsNullOrWhiteSpace(responseFormat))
            {
                content.Add(new StringContent(responseFormat), "response_format");
            }
            if (temperatue != null)
            {
                content.Add(new StringContent(temperatue.ToString()), "temperatue");
            }
            if (!string.IsNullOrWhiteSpace(language))
            {
                content.Add(new StringContent(language), "language");
            }
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TranscriptionResponse>(contentString);
                return result;
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }

        public async Task<TranslationResponse> CreateTranslation(Stream file, string fileName, string model, string prompt, string responseFormat, double? temperatue)
        {
            var url = new Uri(_baseUri, "audio/translations");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var content = new MultipartFormDataContent()
            {
                { new StreamContent(file), "file", fileName }
            };
            if (!string.IsNullOrWhiteSpace(model))
            {
                content.Add(new StringContent(model), "model");
            }
            else
            {
                throw new ArgumentException("Model name can not be null", nameof(model));
            }
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                content.Add(new StringContent(prompt), "prompt");
            }
            if (!string.IsNullOrWhiteSpace(responseFormat))
            {
                content.Add(new StringContent(responseFormat), "response_format");
            }
            if (temperatue != null)
            {
                content.Add(new StringContent(temperatue.ToString()), "temperatue");
            }
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<TranslationResponse>(contentString);
                return result;
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }
    }
}
