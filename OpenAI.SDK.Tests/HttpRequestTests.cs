using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models.Chat;
using OpenAI.SDK.Models.Completions;

namespace OpenAI.SDK.Tests;

public class HttpRequestTests
{
    private const string ChatCompletionJson = """
        {
          "id": "chatcmpl-1",
          "object": "chat.completion",
          "created": 1,
          "choices": [ { "index": 0, "message": { "role": "assistant", "content": "hi" }, "finish_reason": "stop" } ],
          "usage": { "prompt_tokens": 1, "completion_tokens": 1, "total_tokens": 2 }
        }
        """;

    private const string ModelsListJson = """
        { "object": "list", "data": [] }
        """;

    private const string ImageResponseJson = """
        { "created": 1, "data": [] }
        """;

    [Fact]
    public async Task RetrieveFileContent_UsesActualFileIdInRequestUrl()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        });
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();
        var content = await filesApi.RetrieveFileContentAsync("file-123", CancellationToken.None);

        Assert.Equal(new byte[] { 1, 2, 3 }, content);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.openai.com/v1/files/file-123/content", request.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task RetrieveFileContent_ThrowsApiExceptionOnErrorStatus()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            ReasonPhrase = "Not Found",
            Content = new StringContent("{\"error\":\"not found\"}", Encoding.UTF8, "application/json")
        });
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();

        var exception = await Assert.ThrowsAsync<OpenAiApiException>(() => filesApi.RetrieveFileContentAsync("missing", CancellationToken.None));
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Contains("not found", exception.ResponseContents);
    }

    [Fact]
    public async Task ChatApi_NormalizesBaseUrlWithoutTrailingSlash()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler, baseUrl: "https://api.openai.com/v1");

        var chatApi = provider.GetRequiredService<IChatApi>();
        await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.openai.com/v1/chat/completions", request.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task ChatApi_RejectsStreamingRequestBeforeSendingIt()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();

        await Assert.ThrowsAsync<NotSupportedException>(() => chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } },
            Stream = true
        }, CancellationToken.None));

        Assert.Empty(handler.Requests);
    }

#pragma warning disable CS0612 // Retired API must keep working for backward compatibility.
    [Fact]
    public async Task CompletionsApi_RejectsStreamingRequestBeforeSendingIt()
    {
        const string completionJson = """
            {
              "id": "cmpl-1",
              "object": "text_completion",
              "created": 1,
              "model": "davinci",
              "choices": [ { "text": "hi", "index": 0, "logprobs": null, "finish_reason": "stop" } ],
              "usage": { "prompt_tokens": 1, "completion_tokens": 1, "total_tokens": 2 }
            }
            """;
        var handler = new StubHttpMessageHandler(_ => JsonResponse(completionJson));
        using var provider = CreateProvider(handler);

        var completionsApi = provider.GetRequiredService<ICompletionsApi>();

        await Assert.ThrowsAsync<NotSupportedException>(() => completionsApi.CreateCompletionAsync(new ApiCompletionRequest
        {
            Model = "davinci",
            Prompt = new List<string> { "hi" },
            Stream = true
        }, CancellationToken.None));

        Assert.Empty(handler.Requests);
    }
#pragma warning restore CS0612

    [Fact]
    public async Task ChatApi_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, cts.Token);

        Assert.True(handler.LastCancellationToken.CanBeCanceled);
    }

    [Fact]
    public async Task ChatApi_HonorsPreCanceledToken()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, new CancellationToken(canceled: true)));
    }

    [Fact]
    public async Task ModelsApi_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ModelsListJson));
        using var provider = CreateProvider(handler);

        var modelsApi = provider.GetRequiredService<IModelsApi>();
        await modelsApi.GetModelsAsync(cts.Token);

        Assert.True(handler.LastCancellationToken.CanBeCanceled);
    }

    [Fact]
    public async Task RetrieveFileContentAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        });
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();
        await filesApi.RetrieveFileContentAsync("file-123", cts.Token);

        Assert.True(handler.LastCancellationToken.CanBeCanceled);
    }

    [Fact]
    public async Task CreateImageEditAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ImageResponseJson));
        using var provider = CreateProvider(handler);

        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });
        await imagesApi.CreateImageEditAsync(image, "img.png", "a prompt", cancellationToken: cts.Token);

        Assert.True(handler.LastCancellationToken.CanBeCanceled);
    }

    [Fact]
    public async Task ChatApi_UsesDefaultBaseUrl_WhenNotConfigured()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
        });
        services.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(handler));
        services.AddOpenAiApi();
        using var provider = services.BuildServiceProvider();

        var chatApi = provider.GetRequiredService<IChatApi>();
        await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.openai.com/v1/chat/completions", request.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task CreateImageEditAsync_IncludesModelWhenProvided()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(ImageResponseJson);
        });
        using var provider = CreateProvider(handler);

        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });
        await imagesApi.CreateImageEditAsync(image, "img.png", "a prompt", model: "gpt-image-1", cancellationToken: CancellationToken.None);

        Assert.NotNull(requestBody);
        Assert.Contains("name=model", requestBody!);
        Assert.Contains("gpt-image-1", requestBody!);
    }

#pragma warning disable CS0618 // The obsolete overload must keep working for backward compatibility.
    [Fact]
    public async Task ChatApi_ObsoleteOverload_StillSendsRequest()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var result = await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        });

        Assert.NotNull(result);
        Assert.Single(handler.Requests);
    }
#pragma warning restore CS0618

    private static ServiceProvider CreateProvider(HttpMessageHandler handler, string baseUrl = "https://api.openai.com/v1/")
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = baseUrl;
        });
        services.AddSingleton<IHttpClientFactory>(new StubHttpClientFactory(handler));
        services.AddOpenAiApi();
        return services.BuildServiceProvider();
    }

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private sealed class StubHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public StubHttpClientFactory(HttpMessageHandler handler) => _handler = handler;

        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

        public List<HttpRequestMessage> Requests { get; } = new();

        public CancellationToken LastCancellationToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            LastCancellationToken = cancellationToken;
            return Task.FromResult(_responder(request));
        }
    }
}
