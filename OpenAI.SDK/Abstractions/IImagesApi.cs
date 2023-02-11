using OpenAI.SDK.Models.CreateImage;
using System.IO;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IImagesApi
    {
        Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request);

        Task<CreateImageResponse> CreateImageEditAsync(
            Stream image, string prompt, Stream mask = null, int? n = null, string size = null,
            string responseFormat = null, string user = null);

        Task<CreateImageResponse> CreateImageVariation(
            Stream image, int? n = null, string size = null, string responseFormat = null, string user = null);
    }
}