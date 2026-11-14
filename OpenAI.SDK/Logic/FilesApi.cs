using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class FilesApi : BaseApi, IFilesApi
    {
        public FilesApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<List<FileDetails>> ListFilesAsync()
        {
            return ListFilesAsync(CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<List<FileDetails>> ListFilesAsync(CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ListResponse<FileDetails>>(HttpMethod.Get, $"files", cancellationToken);
            return response.Data;
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<DeleteFileResponse> DeleteFileAsync(string fileId)
        {
            return DeleteFileAsync(fileId, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<DeleteFileResponse> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<DeleteFileResponse>(HttpMethod.Delete, $"files/{fileId}", cancellationToken);
            return response;
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<FileDetails> RetrieveFileAsync(string fileId)
        {
            return RetrieveFileAsync(fileId, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<FileDetails> RetrieveFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<FileDetails>(HttpMethod.Get, $"files/{fileId}", cancellationToken);
            return response;
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<byte[]> RetrieveFileContentAsync(string fileId)
        {
            return RetrieveFileContentAsync(fileId, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<byte[]> RetrieveFileContentAsync(string fileId, CancellationToken cancellationToken = default)
        {
            var url = new Uri(_baseUri, $"files/{fileId}/content");
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream, 81920, cancellationToken);
                    return memoryStream.ToArray();
                }
            }

            throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
        }

        /// <inheritdoc/>
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose)
        {
            return UploadFileAsync(file, fileName, purpose, CancellationToken.None);
        }

        /// <inheritdoc/>
        public Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose, CancellationToken cancellationToken = default)
        {
            if (file == null) throw new ArgumentException("File is required", nameof(file));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Filename is required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Filename is purpose", nameof(purpose));

            return UploadFileImplAsync(file, fileName, purpose, cancellationToken);
        }

        private async Task<FileDetails> UploadFileImplAsync(Stream file, string fileName, string purpose, CancellationToken cancellationToken)
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
            var response = await client.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    return await JsonSerializer.DeserializeAsync<FileDetails>(stream, cancellationToken: cancellationToken);
                }
            }
            else
            {
                throw new OpenAiApiException(response.StatusCode, await response.Content.ReadAsStringAsync(), response.ReasonPhrase);
            }
        }
    }
}
