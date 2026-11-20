using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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
        public async Task<List<FileDetails>> ListFilesAsync(CancellationToken cancellationToken = default)
        {
            var response = await ExecuteRequest<ListResponse<FileDetails>>(HttpMethod.Get, "files", cancellationToken).ConfigureAwait(false);
            return response != null ? response.Data : null;
        }

        /// <inheritdoc/>
        public async Task<DeleteFileResponse> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new ArgumentException("File id is required.", nameof(fileId));
            }
            var response = await ExecuteRequest<DeleteFileResponse>(HttpMethod.Delete, $"files/{fileId}", cancellationToken).ConfigureAwait(false);
            return response;
        }

        /// <inheritdoc/>
        public async Task<FileDetails> RetrieveFileAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new ArgumentException("File id is required.", nameof(fileId));
            }
            var response = await ExecuteRequest<FileDetails>(HttpMethod.Get, $"files/{fileId}", cancellationToken).ConfigureAwait(false);
            return response;
        }

        /// <inheritdoc/>
        public async Task<byte[]> RetrieveFileContentAsync(string fileId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileId))
            {
                throw new ArgumentException("File id is required.", nameof(fileId));
            }
            var content = await ExecuteRawRequestAsync(HttpMethod.Get, $"files/{fileId}/content", cancellationToken).ConfigureAwait(false);
            return content;
        }

        /// <inheritdoc/>
        public Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose, CancellationToken cancellationToken = default)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Filename is required", nameof(fileName));
            if (string.IsNullOrWhiteSpace(purpose)) throw new ArgumentException("Purpose is required", nameof(purpose));

            var content = new MultipartFormDataContent
            {
                { new StreamContent(new NonDisposingStream(file)), "file", fileName },
                { new StringContent(purpose), "purpose" }
            };
            return ExecuteMultipartRequestAsync<FileDetails>(HttpMethod.Post, "files", content, cancellationToken);
        }
    }
}