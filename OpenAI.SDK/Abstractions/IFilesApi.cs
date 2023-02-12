using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Files;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IFilesApi
    {
        Task<List<FileDetails>> ListFilesAsync();
        Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose);
        Task<DeleteFileResponse> DeleteFileAsync(string fileId);
        Task<FileDetails> RetrieveFileAsync(string fileId);
        Task<byte[]> RetrieveFileContentAsync(string fileId);
    }
}
