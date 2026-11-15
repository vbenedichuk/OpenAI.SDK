using OpenAI.SDK.Models.CreateImage;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request);

        /// <summary>
        /// POST https://api.openai.com/v1/images/generations
        /// Creates an image given a prompt.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST https://api.openai.com/v1/images/edits
        /// Creates an edited or extended image given an original image and a prompt.
        /// </summary>
        /// <param name="image">The image to edit.</param>
        /// <param name="imageFileName">Image filename</param>
        /// <param name="prompt">A text description of the desired image(s).</param>
        /// <param name="mask">An optional mask image.</param>
        /// <param name="maskFileName">Mask filename</param>
        /// <param name="n">The number of images to generate.</param>
        /// <param name="size">The size of the generated images.</param>
        /// <param name="responseFormat">The format in which the generated images are returned (legacy models).</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="model">The image model to use (for example, gpt-image-2).</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        /// <remarks>
        /// The <paramref name="image"/> and <paramref name="mask"/> streams are owned by the caller:
        /// this method reads from their current position and does not dispose them.
        /// </remarks>
        Task<CreateImageResponse> CreateImageEditAsync(
            Stream image, string imageFileName, string prompt, Stream mask = null, string maskFileName = null, int? n = null, string size = null,
            string responseFormat = null, string user = null, string model = null, CancellationToken cancellationToken = default);

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
        [Obsolete("The /images/variations endpoint has been retired. Use CreateImageEditAsync with a GPT Image model instead.")]
        Task<CreateImageResponse> CreateImageVariation(
            Stream image, string imageFileName, int? n = null, string size = null, string responseFormat = null, string user = null);

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
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns></returns>
        /// <remarks>
        /// The <paramref name="image"/> stream is owned by the caller: this method reads from
        /// its current position and does not dispose it.
        /// </remarks>
        [Obsolete("The /images/variations endpoint has been retired. Use CreateImageEditAsync with a GPT Image model instead.")]
        Task<CreateImageResponse> CreateImageVariationAsync(
            Stream image, string imageFileName, int? n = null, string size = null, string responseFormat = null, string user = null,
            CancellationToken cancellationToken = default);
    }
}
