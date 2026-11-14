using OpenAI.SDK.Models;
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
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<List<FileDetails>> ListFilesAsync();

        Task<List<FileDetails>> ListFilesAsync(CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose);

        /// <summary>
        /// Uploads a file that can be used across various endpoints.
        /// </summary>
        /// <remarks>
        /// The stream is owned by the caller: this method reads from its current position
        /// and does not dispose it.
        /// </remarks>
        Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<DeleteFileResponse> DeleteFileAsync(string fileId);

        Task<DeleteFileResponse> DeleteFileAsync(string fileId, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<FileDetails> RetrieveFileAsync(string fileId);

        Task<FileDetails> RetrieveFileAsync(string fileId, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<byte[]> RetrieveFileContentAsync(string fileId);

        Task<byte[]> RetrieveFileContentAsync(string fileId, CancellationToken cancellationToken = default);
    }
}
