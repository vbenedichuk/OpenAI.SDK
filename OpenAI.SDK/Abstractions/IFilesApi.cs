using OpenAI.SDK.Models.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IFilesApi
    {
        Task<List<FileDetails>> ListFilesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a file that can be used across various endpoints.
        /// </summary>
        /// <remarks>
        /// The stream is owned by the caller: this method reads from its current position
        /// and does not dispose it.
        /// </remarks>
        Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose, CancellationToken cancellationToken = default);


        Task<DeleteFileResponse> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);

        Task<FileDetails> RetrieveFileAsync(string fileId, CancellationToken cancellationToken = default);

        Task<byte[]> RetrieveFileContentAsync(string fileId, CancellationToken cancellationToken = default);
    }
}
