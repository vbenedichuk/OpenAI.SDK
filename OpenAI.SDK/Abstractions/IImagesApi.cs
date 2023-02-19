using OpenAI.SDK.Models.CreateImage;
using System.IO;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Given a prompt and/or an input image, the model will generate a new image.
    /// Related guide: <see href="https://platform.openai.com/docs/guides/images">Image generation</see>
    /// </summary>
    public interface IImagesApi
    {
        /// <summary>
        /// POST https://api.openai.com/v1/images/generations
        /// Creates an image given a prompt.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request);

        /// <summary>
        /// POST https://api.openai.com/v1/images/edits
        /// Creates an edited or extended image given an original image and a prompt.
        /// </summary>
        /// <param name="image">The image to edit. Must be a valid PNG file, less than 4MB, and square. If mask is not provided, image must have transparency, which will be used as the mask.</param>
        /// <param name="imageFileName">Image filename</param>
        /// <param name="prompt">A text description of the desired image(s). The maximum length is 1000 characters.</param>
        /// <param name="mask">An additional image whose fully transparent areas (e.g. where alpha is zero) indicate where image should be edited. Must be a valid PNG file, less than 4MB, and have the same dimensions as image.</param>
        /// <param name="maskFileName">Mask filename</param>
        /// <param name="n">The number of images to generate. Must be between 1 and 10.</param>
        /// <param name="size">The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024</param>
        /// <param name="responseFormat">The format in which the generated images are returned. Must be one of url or b64_json</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse. <see href="https://platform.openai.com/docs/guides/safety-best-practices/end-user-ids">Learn more</see>.</param>
        /// <returns></returns>
        Task<CreateImageResponse> CreateImageEditAsync(
            Stream image, string imageFileName, string prompt, Stream mask = null, string maskFileName = null, int? n = null, string size = null,
            string responseFormat = null, string user = null);
        /// <summary>
        /// POST https://api.openai.com/v1/images/variations
        /// Creates a variation of a given image.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageFileName"></param>
        /// <param name="n"></param>
        /// <param name="size"></param>
        /// <param name="responseFormat"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<CreateImageResponse> CreateImageVariation(
            Stream image, string imageFileName, int? n = null, string size = null, string responseFormat = null, string user = null);
    }
}