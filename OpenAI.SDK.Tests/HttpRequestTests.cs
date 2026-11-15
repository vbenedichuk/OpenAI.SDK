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
    public async Task RetrieveFileContentAsync_TruncatesLargeErrorBodyTo64KB()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(new string('x', 100_000), Encoding.UTF8, "application/json")
        });
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();

        var exception = await Assert.ThrowsAsync<OpenAiApiException>(() => filesApi.RetrieveFileContentAsync("file-1", CancellationToken.None));
        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Equal(65536, exception.ResponseContents.Length);
    }

    [Fact]
    public async Task CreateImageEditAsync_WrapsMalformedJsonInResponseFormatException()
    {
        const string malformed = "{not-json";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(malformed, Encoding.UTF8, "application/json")
        });
        using var provider = CreateProvider(handler);

        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });
        var exception = await Assert.ThrowsAsync<OpenAiResponseFormatException>(() => imagesApi.CreateImageEditAsync(image, "img.png", "a prompt", cancellationToken: CancellationToken.None));

        Assert.Equal(malformed, exception.ResponseContents);
        Assert.Equal(HttpStatusCode.OK, exception.StatusCode);
        Assert.IsType<JsonException>(exception.InnerException);
        Assert.EndsWith("/v1/images/edits", exception.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task ChatApi_NormalizesBaseUrlWithoutTrailingSlash()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler, out var factory, baseUrl: "https://api.openai.com/v1");

        var chatApi = provider.GetRequiredService<IChatApi>();
        await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.openai.com/v1/chat/completions", request.RequestUri!.AbsoluteUri);
        Assert.Equal(ConfigurationExtensions.HttpClientName, factory.LastName);
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
        var handler = new BlockingHandler();
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var task = chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, cts.Token);

        await handler.Entered;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.True(handler.LastToken.IsCancellationRequested);
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
        var handler = new BlockingHandler();
        using var provider = CreateProvider(handler);

        var modelsApi = provider.GetRequiredService<IModelsApi>();
        var task = modelsApi.GetModelsAsync(cts.Token);

        await handler.Entered;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.True(handler.LastToken.IsCancellationRequested);
    }

    [Fact]
    public async Task RetrieveFileContentAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var handler = new BlockingHandler();
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();
        var task = filesApi.RetrieveFileContentAsync("file-123", cts.Token);

        await handler.Entered;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.True(handler.LastToken.IsCancellationRequested);
    }

    [Fact]
    public async Task CreateImageEditAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var handler = new BlockingHandler();
        using var provider = CreateProvider(handler);

        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });
        var task = imagesApi.CreateImageEditAsync(image, "img.png", "a prompt", cancellationToken: cts.Token);

        await handler.Entered;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.True(handler.LastToken.IsCancellationRequested);
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
        string? model = null;
        string? prompt = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            var form = Assert.IsType<MultipartFormDataContent>(request.Content);
            foreach (var part in form)
            {
                var name = part.Headers.ContentDisposition?.Name?.Trim('"');
                if (name == "model")
                {
                    model = part.ReadAsStringAsync().GetAwaiter().GetResult();
                }
                else if (name == "prompt")
                {
                    prompt = part.ReadAsStringAsync().GetAwaiter().GetResult();
                }
            }
            return JsonResponse(ImageResponseJson);
        });
        using var provider = CreateProvider(handler);

        var imagesApi = provider.GetRequiredService<IImagesApi>();
        using var image = new MemoryStream(new byte[] { 1, 2, 3 });
        await imagesApi.CreateImageEditAsync(image, "img.png", "a prompt", model: "gpt-image-2", cancellationToken: CancellationToken.None);

        Assert.Equal("gpt-image-2", model);
        Assert.Equal("a prompt", prompt);
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

    [Fact]
    public async Task ChatApi_ThrowsApiExceptionOnErrorStatus()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            ReasonPhrase = "Unauthorized",
            Content = new StringContent("{\"error\":{\"message\":\"invalid api key\"}}", Encoding.UTF8, "application/json")
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();

        var exception = await Assert.ThrowsAsync<OpenAiApiException>(() => chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None));

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Contains("invalid api key", exception.ResponseContents);
        Assert.Contains("401", exception.Message);
    }

    [Fact]
    public async Task ChatApi_SendsExpectedRequestShape()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonResponse(ChatCompletionJson);
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } },
            Temperature = null
        }, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("test-key", request.Headers.Authorization.Parameter);
        Assert.NotNull(request.Content);
        Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);

        Assert.NotNull(requestBody);
        using var document = JsonDocument.Parse(requestBody);
        Assert.Equal("gpt-3.5-turbo", document.RootElement.GetProperty("model").GetString());
        Assert.True(document.RootElement.TryGetProperty("messages", out _));
        Assert.False(document.RootElement.TryGetProperty("temperature", out _));
    }

    [Fact]
    public async Task ChatApi_ReturnsDefault_WhenResponseBodyEmpty()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var result = await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ChatApi_ReturnsDefault_WhenEmptyBodyHasNoContentLength()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            // No Content-Length and a non-seekable stream, like a chunked response.
            Content = new StreamContent(new NonSeekableStream(Array.Empty<byte>()))
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var result = await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ChatApi_WrapsMalformedJsonInApiException()
    {
        const string malformed = "{not-json";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StreamContent(new NonSeekableStream(Encoding.UTF8.GetBytes(malformed)))
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var exception = await Assert.ThrowsAsync<OpenAiResponseFormatException>(() => chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None));

        Assert.IsType<JsonException>(exception.InnerException);
        Assert.Equal(malformed, exception.ResponseContents);
        Assert.Equal(HttpStatusCode.OK, exception.StatusCode);
        Assert.Contains("Failed to deserialize", exception.Message);
        Assert.EndsWith("/v1/chat/completions", exception.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task ChatApi_AppliesOptionsTimeoutToWholeRequest()
    {
        var handler = new BlockingBodyHandler();
        using var provider = CreateProvider(handler, timeout: TimeSpan.FromMilliseconds(100));
        using var callerCts = new CancellationTokenSource();

        var chatApi = provider.GetRequiredService<IChatApi>();
        var task = chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, callerCts.Token);

        await handler.ReadEntered.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.ThrowsAsync<OpenAiTimeoutException>(() => task);

        Assert.False(callerCts.IsCancellationRequested);
        Assert.True(handler.ReadToken.IsCancellationRequested);
    }

    [Fact]
    public async Task ChatApi_TimeoutExceptionIncludesRequestContext()
    {
        var handler = new BlockingBodyHandler();
        using var provider = CreateProvider(handler, timeout: TimeSpan.FromMilliseconds(100));
        using var callerCts = new CancellationTokenSource();

        var chatApi = provider.GetRequiredService<IChatApi>();
        var task = chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, callerCts.Token);

        await handler.ReadEntered.WaitAsync(TimeSpan.FromSeconds(5));
        var exception = await Assert.ThrowsAsync<OpenAiTimeoutException>(() => task);

        Assert.Equal(TimeSpan.FromMilliseconds(100), exception.Timeout);
        Assert.NotNull(exception.RequestUri);
        Assert.EndsWith("/v1/chat/completions", exception.RequestUri!.AbsoluteUri);
        Assert.Contains("/v1/chat/completions", exception.Message);
    }

    [Fact]
    public async Task ChatApi_AcceptsTimeoutLongerThanCancelAfterMaximum()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler, timeout: TimeSpan.FromDays(50));

        var chatApi = provider.GetRequiredService<IChatApi>();
        var result = await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("chatcmpl-1", result.Id);
    }

    [Fact]
    public async Task RetrieveFile_EscapesIdInRequestUrl()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{\"id\":\"x\"}"));
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();
        await filesApi.RetrieveFileAsync("a b#c", CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.openai.com/v1/files/a%20b%23c", request.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task RetrieveFile_RejectsPathTraversal()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{\"id\":\"x\"}"));
        using var provider = CreateProvider(handler);

        var filesApi = provider.GetRequiredService<IFilesApi>();
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => filesApi.RetrieveFileAsync("../../models", CancellationToken.None));

        Assert.IsNotType<ArgumentNullException>(exception);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task OpenAiApiException_ExposesRequestUriAndRetryAfter()
    {
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage((HttpStatusCode)429)
            {
                ReasonPhrase = "Too Many Requests",
                Content = new StringContent("{\"error\":{\"message\":\"slow down\"}}", Encoding.UTF8, "application/json")
            };
            response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(7));
            return response;
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var exception = await Assert.ThrowsAsync<OpenAiApiException>(() => chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None));

        Assert.Equal(TimeSpan.FromSeconds(7), exception.RetryAfter);
        Assert.Equal("https://api.openai.com/v1/chat/completions", exception.RequestUri!.AbsoluteUri);
        Assert.NotNull(exception.ResponseHeaders);
        Assert.Contains("slow down", exception.Message);
        Assert.DoesNotContain("Too Many Requests", exception.Message);
        Assert.Contains("429", exception.Message);
    }

    [Fact]
    public async Task OpenAiApiException_TruncatesLongBodyButKeepsResponseContents()
    {
        var body = new string('x', 2000);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var exception = await Assert.ThrowsAsync<OpenAiApiException>(() => chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None));

        Assert.Equal(body, exception.ResponseContents);
        Assert.Contains("(truncated)", exception.Message);
        Assert.True(exception.Message.Length < body.Length);
    }

    [Fact]
    public async Task ChatApi_ReturnsStringContentFromResponse()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson));
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var result = await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        var message = Assert.Single(result!.Choices).Message;
        Assert.IsType<string>(message.Content);
        Assert.Equal("hi", message.Content as string);
        Assert.Equal("hi", message.ContentText);
    }

    [Fact]
    public async Task ChatApi_ReturnsMultiPartContentAsJsonElement()
    {
        const string multiPartJson = """
            {
              "id": "chatcmpl-1",
              "object": "chat.completion",
              "created": 1,
              "choices": [ { "index": 0, "message": { "role": "assistant", "content": [ { "type": "text", "text": "hi" } ] }, "finish_reason": "stop" } ]
            }
            """;
        var handler = new StubHttpMessageHandler(_ => JsonResponse(multiPartJson));
        using var provider = CreateProvider(handler);

        var chatApi = provider.GetRequiredService<IChatApi>();
        var result = await chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage> { new() { Role = "user", Content = "hi" } }
        }, CancellationToken.None);

        var content = Assert.Single(result!.Choices).Message.Content;
        var element = Assert.IsType<JsonElement>(content);
        Assert.Equal(JsonValueKind.Array, element.ValueKind);
    }

    [Fact]
    public async Task ChatApi_ThrowsArgumentNullException_WhenRequestIsNull()
    {
        using var provider = CreateProvider(new StubHttpMessageHandler(_ => JsonResponse(ChatCompletionJson)));
        var chatApi = provider.GetRequiredService<IChatApi>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => chatApi.CreateCompletionAsync(null, CancellationToken.None));
    }

    private sealed class BlockingHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource<bool> _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Entered => _entered.Task;

        public CancellationToken LastToken { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastToken = cancellationToken;
            _entered.TrySetResult(true);
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    private sealed class BlockingBodyHandler : HttpMessageHandler
    {
        private readonly TaskCompletionSource<bool> _readEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ReadEntered => _readEntered.Task;

        public CancellationToken ReadToken { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(new BlockingReadStream(_readEntered, token => ReadToken = token))
            };
            return Task.FromResult(response);
        }
    }

    private sealed class BlockingReadStream : Stream
    {
        private readonly TaskCompletionSource<bool> _entered;
        private readonly Action<CancellationToken> _onRead;

        public BlockingReadStream(TaskCompletionSource<bool> entered, Action<CancellationToken> onRead)
        {
            _entered = entered;
            _onRead = onRead;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _onRead(cancellationToken);
            _entered.TrySetResult(true);
            return WaitAsync(cancellationToken);
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _onRead(cancellationToken);
            _entered.TrySetResult(true);
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        private static async Task<int> WaitAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class NonSeekableStream : Stream
    {
        private readonly byte[] _buffer;
        private int _position;

        public NonSeekableStream(byte[] buffer) => _buffer = buffer;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = _buffer.Length - _position;
            if (remaining <= 0)
            {
                return 0;
            }

            var length = Math.Min(remaining, count);
            Array.Copy(_buffer, _position, buffer, offset, length);
            _position += length;
            return length;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }


    private static ServiceProvider CreateProvider(HttpMessageHandler handler, string baseUrl = "https://api.openai.com/v1/", TimeSpan? timeout = null)
        => CreateProvider(handler, out _, baseUrl, timeout);

    private static ServiceProvider CreateProvider(HttpMessageHandler handler, out StubHttpClientFactory factory, string baseUrl = "https://api.openai.com/v1/", TimeSpan? timeout = null)
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = baseUrl;
            if (timeout.HasValue)
            {
                options.Timeout = timeout.Value;
            }
        });
        factory = new StubHttpClientFactory(handler);
        services.AddSingleton<IHttpClientFactory>(factory);
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

        public string? LastName { get; private set; }

        public HttpClient CreateClient(string name)
        {
            LastName = name;
            return new(_handler, disposeHandler: false);
        }
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
