using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models.CreateImage;
using OpenAI.SDK.Models.Files;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OpenAI.SDK.Logic
{
    internal class OpenApiFiles : BaseApi, IOpenApiFiles
    {
        public OpenApiFiles(IHttpClientFactory clientFactory,
            IOptions<OpenApiOptions> options) : base(clientFactory, options)
        {
        }

        /// <inheritdoc/>
        public async Task<ListFilesResponse> ListFilesAsync()
        {
            var response = await ExecuteRequest<ListFilesResponse>(HttpMethod.Get, $"files");
            return response;
        }

        /// <inheritdoc/>
        public async Task<DeleteFileResponse> DeleteFileAsync(string fileId)
        {
            var response = await ExecuteRequest<DeleteFileResponse>(HttpMethod.Delete, $"files/{fileId}");
            return response;
        }

        /// <inheritdoc/>
        public async Task<FileDetails> RetrieveFileAsync(string fileId)
        {
            var response = await ExecuteRequest<FileDetails>(HttpMethod.Get, $"files/{fileId}");
            return response;
        }

        /// <inheritdoc/>
        public async Task<byte[]> RetrieveFileContentAsync(string fileId)
        {
            var url = new Uri(_baseUri, "files/{fileId}/content");
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsByteArrayAsync();
        }

        /// <inheritdoc/>
        public Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose)
        {
            if (file == null) throw new ArgumentException("File is required", nameof(file));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Filename is required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Filename is purpose", nameof(purpose));

            return UploadFileImplAsync(file, fileName, purpose);
        }

        private async Task<FileDetails> UploadFileImplAsync(Stream file, string fileName, string purpose)
        {
            var url = new Uri(_baseUri, "files");
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var content = new MultipartFormDataContent
            {
                { new StreamContent(file), "file", fileName },
                { new StringContent(purpose), "purpose" }
            };
            request.Content = content;
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var contentString = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FileDetails>(contentString);
                return result;
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }
    }
}
