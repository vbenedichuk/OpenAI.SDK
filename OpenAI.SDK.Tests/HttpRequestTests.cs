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

    [Fact]
    public async Task RetrieveFileContent_UsesActualFileIdInRequestUrl()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(new byte[] { 1, 2, 3 })
        });
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();
        var content = await filesApi.RetrieveFileContentAsync("file-123");

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

        var exception = await Assert.ThrowsAsync<OpenAiApiException>(() => filesApi.RetrieveFileContentAsync("missing"));
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
        });

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
        }));

        Assert.Empty(handler.Requests);
    }

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
        }));

        Assert.Empty(handler.Requests);
    }

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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responder(request));
        }
    }
}
