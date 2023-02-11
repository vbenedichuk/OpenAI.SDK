using OpenAI.SDK.Models.Files;
using System.IO;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IOpenApiFiles
    {
        Task<ListFilesResponse> ListFilesAsync();
        Task<FileDetails> UploadFileAsync(Stream file, string fileName, string purpose);
        Task<DeleteFileResponse> DeleteFileAsync(string fileId);
        Task<FileDetails> RetrieveFileAsync(string fileId);
        Task<byte[]> RetrieveFileContentAsync(string fileId);
    }
}
