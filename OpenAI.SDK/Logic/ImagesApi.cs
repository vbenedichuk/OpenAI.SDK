using Microsoft.Extensions.Options;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models.CreateImage;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Logic
{
    internal class ImagesApi : BaseApi, IImagesApi
    {
        public ImagesApi(IHttpClientFactory clientFactory,
            IOptions<OpenAiOptions> options) : base(clientFactory, options)
        {
        }

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        public Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request)
        {
            return CreateImageAsync(request, CancellationToken.None);
        }

        public async Task<CreateImageResponse> CreateImageAsync(CreateImageRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(request));
            }
            var response = await ExecuteRequest<CreateImageRequest, CreateImageResponse>(HttpMethod.Post, "images/generations", request, cancellationToken).ConfigureAwait(false);
            return response;
        }

        public Task<CreateImageResponse> CreateImageEditAsync(Stream image, string imageFileName, string prompt, Stream mask = null,
            string maskFileName = null, int? n = null, string size = null, string responseFormat = null, string user = null,
            string model = null, CancellationToken cancellationToken = default)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException("Image file name is required.", nameof(imageFileName));
            }
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt is required.", nameof(prompt));
            }

            var content = PrepareRequestContent(image, imageFileName, prompt, mask, maskFileName, n, size, responseFormat, user, model);
            return ExecuteMultipartRequestAsync<CreateImageResponse>(HttpMethod.Post, "images/edits", content, cancellationToken);
        }

        [Obsolete("The /images/variations endpoint has been retired. Use CreateImageEditAsync with a GPT Image model instead.")]
        public Task<CreateImageResponse> CreateImageVariation(Stream image, string imageFileName, int? n = null, string size = null,
            string responseFormat = null, string user = null)
        {
            return CreateImageVariationAsync(image, imageFileName, n, size, responseFormat, user, CancellationToken.None);
        }

        [Obsolete("The /images/variations endpoint has been retired. Use CreateImageEditAsync with a GPT Image model instead.")]
        public Task<CreateImageResponse> CreateImageVariationAsync(Stream image, string imageFileName, int? n = null, string size = null,
            string responseFormat = null, string user = null, CancellationToken cancellationToken = default)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }
            if (string.IsNullOrWhiteSpace(imageFileName))
            {
                throw new ArgumentException("Image file name is required.", nameof(imageFileName));
            }

            var content = PrepareRequestContent(image, imageFileName, null, null, null, n, size, responseFormat, user, null);
            return ExecuteMultipartRequestAsync<CreateImageResponse>(HttpMethod.Post, "images/variations", content, cancellationToken);
        }

        private static MultipartFormDataContent PrepareRequestContent(Stream image, string imageFileName, string prompt, Stream mask = null,
            string maskFileName = null, int? n = null, string size = null, string responseFormat = null, string user = null, string model = null)
        {
            var content = new MultipartFormDataContent()
            {
                { new StreamContent(new NonDisposingStream(image)), "image", imageFileName }
            };
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                content.Add(new StringContent(prompt), "prompt");
            }
            if (mask != null)
            {
                content.Add(new StreamContent(new NonDisposingStream(mask)), "mask", maskFileName);
            }
            if (n != null)
            {
                content.Add(new StringContent(n.Value.ToString()), "n");
            }
            if (!string.IsNullOrWhiteSpace(size))
            {
                content.Add(new StringContent(size), "size");
            }
            if (!string.IsNullOrWhiteSpace(responseFormat))
            {
                content.Add(new StringContent(responseFormat), "response_format");
            }
            if (user != null)
            {
                content.Add(new StringContent(user), "user");
            }
            if (!string.IsNullOrWhiteSpace(model))
            {
                content.Add(new StringContent(model), "model");
            }
            return content;
        }
    }
}