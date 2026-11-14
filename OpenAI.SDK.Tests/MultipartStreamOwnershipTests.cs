using System.Net;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;

namespace OpenAI.SDK.Tests;

public class MultipartStreamOwnershipTests
{
    private const string FileDetailsJson = """
        {
          "id": "file-1",
          "object": "file",
          "bytes": 3,
          "created_at": 1,
          "filename": "test.txt",
          "purpose": "fine-tune"
        }
        """;

    private const string ImageResponseJson = """
        { "created": 1, "data": [] }
        """;

    [Fact]
    public async Task UploadFileAsync_DoesNotDisposeCallerStream()
    {
        var handler = new StubHttpMessageHandler(FileDetailsJson);
        using var provider = CreateProvider(handler);
        var filesApi = provider.GetRequiredService<IFilesApi>();
        using var file = new MemoryStream(new byte[] { 1, 2, 3 });

        await filesApi.UploadFileAsync(file, "test.txt", "fine-tune", CancellationToken.None);

        Assert.True(file.CanRead);
        Assert.Equal(3, file.Length);
        Assert.Contains("name=file", handler.RequestBody);
        Assert.Contains("name=purpose", handler.RequestBody);
    }

    [Fact]
    public async Task CreateImageEditAsync_DoesNotDisposeCallerStreams()
    {
        var handler = new StubHttpMessageHandler(ImageResponseJson);
        using var provider = CreateProvider(handler);
        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });
        using var mask = new MemoryStream(new byte[] { 4, 5, 6 });

        await imagesApi.CreateImageEditAsync(
            image, "img.png", "a prompt", mask, "mask.png", cancellationToken: CancellationToken.None);

        Assert.True(image.CanRead);
        Assert.True(mask.CanRead);
        Assert.Contains("name=image", handler.RequestBody);
        Assert.Contains("name=mask", handler.RequestBody);
        Assert.Contains("a prompt", handler.RequestBody);
    }

#pragma warning disable CS0618 // Retired API must keep working for backward compatibility.
    [Fact]
    public async Task CreateImageVariationAsync_DoesNotDisposeCallerStream()
    {
        var handler = new StubHttpMessageHandler(ImageResponseJson);
        using var provider = CreateProvider(handler);
        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });

        await imagesApi.CreateImageVariationAsync(image, "img.png", cancellationToken: CancellationToken.None);

        Assert.True(image.CanRead);
        Assert.Equal(3, image.Length);
        Assert.Contains("name=image", handler.RequestBody);
    }
#pragma warning restore CS0618

    private static ServiceProvider CreateProvider(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.openai.com/v1/";
        });
        services.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(handler));
        services.AddOpenAiApi();
        return services.BuildServiceProvider();
    }

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _json;

        public StubHttpMessageHandler(string json) => _json = json;

        public List<HttpRequestMessage> Requests { get; } = new();

        public string RequestBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (request.Content != null)
            {
                RequestBody = await request.Content.ReadAsStringAsync();
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };
        }
    }
}
