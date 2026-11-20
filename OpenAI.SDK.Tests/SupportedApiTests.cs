using System;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;
using OpenAI.SDK.Exceptions;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.CreateImage;
using OpenAI.SDK.Models.Embeddings;
using OpenAI.SDK.Models.Files;
using OpenAI.SDK.Models.Moderations;

namespace OpenAI.SDK.Tests;

public class SupportedApiTests
{
    [Fact]
    public async Task CreateEmbeddingsAsync_SendsExpectedRequest()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestBody = ReadJsonBody(request);
            return JsonResponse("{ \"object\": \"list\", \"data\": [], \"model\": \"text-embedding-3-small\", \"usage\": { \"prompt_tokens\": 3, \"total_tokens\": 3 } }");
        });
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IEmbeddingsApi>();
        await api.CreateEmbeddingsAsync(new EmbeddingsRequest { Model = "text-embedding-3-small", Input = new[] { "a", "b" } }, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.openai.com/v1/embeddings", request.RequestUri!.AbsoluteUri);
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal("test-key", request.Headers.Authorization.Parameter);

        Assert.NotNull(requestBody);
        using var document = JsonDocument.Parse(requestBody);
        Assert.Equal("text-embedding-3-small", document.RootElement.GetProperty("model").GetString());
        var input = document.RootElement.GetProperty("input");
        Assert.Equal(JsonValueKind.Array, input.ValueKind);
        Assert.Equal(2, input.GetArrayLength());
        Assert.Equal("a", input[0].GetString());
        Assert.Equal("b", input[1].GetString());
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_DeserializesResponse()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{ \"object\": \"list\", \"data\": [ { \"object\": \"embedding\", \"embedding\": [0.1, 0.2], \"index\": 0 } ], \"model\": \"text-embedding-3-small\", \"usage\": { \"prompt_tokens\": 3, \"total_tokens\": 3 } }"));
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IEmbeddingsApi>();
        var result = await api.CreateEmbeddingsAsync(new EmbeddingsRequest { Model = "text-embedding-3-small", Input = new[] { "a", "b" } }, CancellationToken.None);

        Assert.Equal("text-embedding-3-small", result.Model);
        Assert.Single(result.Data);
        var item = result.Data[0];
        Assert.Equal(0, item.Index);
        Assert.Equal(0.1, item.Embedding[0]);
        Assert.Equal(0.2, item.Embedding[1]);
        Assert.Equal(3, result.Usage.TotalTokens);
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_RejectsNullRequest()
    {
        using var provider = CreateProvider(new StubHttpMessageHandler(_ => JsonResponse("{}")));
        var api = provider.GetRequiredService<IEmbeddingsApi>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => api.CreateEmbeddingsAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_RejectsEmptyInput()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IEmbeddingsApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.CreateEmbeddingsAsync(new EmbeddingsRequest { Model = "m", Input = Array.Empty<string>() }, CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CreateEmbeddingsAsync_RejectsBlankModel()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IEmbeddingsApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.CreateEmbeddingsAsync(new EmbeddingsRequest { Model = "  ", Input = new[] { "a" } }, CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CreateModerationAsync_SendsExpectedRequest()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestBody = ReadJsonBody(request);
            return JsonResponse("{ \"id\": \"modr-1\", \"model\": \"omni-moderation-latest\", \"results\": [] }");
        });
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IModerationsApi>();
        await api.CreateModerationAsync("hello", "omni-moderation-latest", CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.openai.com/v1/moderations", request.RequestUri!.AbsoluteUri);

        Assert.NotNull(requestBody);
        using var document = JsonDocument.Parse(requestBody);
        Assert.Equal("omni-moderation-latest", document.RootElement.GetProperty("model").GetString());
        var input = document.RootElement.GetProperty("input");
        Assert.Equal(JsonValueKind.Array, input.ValueKind);
        Assert.Equal(1, input.GetArrayLength());
        Assert.Equal("hello", input[0].GetString());
    }

    [Fact]
    public async Task CreateModerationAsync_DeserializesResponse()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{ \"id\": \"modr-1\", \"model\": \"omni-moderation-latest\", \"results\": [ { \"flagged\": true, \"categories\": { \"violence\": true }, \"category_scores\": { \"violence\": 0.9 } } ] }"));
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IModerationsApi>();
        var result = await api.CreateModerationAsync("hello", "omni-moderation-latest", CancellationToken.None);

        Assert.Equal("modr-1", result.Id);
        Assert.Equal("omni-moderation-latest", result.Model);
        Assert.Single(result.Results);
        var r = result.Results[0];
        Assert.True(r.Flagged);
        Assert.True(r.Categories["violence"]);
        Assert.Equal(0.9, r.CategoryScores["violence"]);
    }

    [Fact]
    public async Task CreateModerationAsync_RejectsNullArray()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IModerationsApi>();

        string[] input = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => api.CreateModerationAsync(input, cancellationToken: CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CreateModerationAsync_RejectsEmptyArray()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IModerationsApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.CreateModerationAsync(Array.Empty<string>(), cancellationToken: CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CreateModerationAsync_RejectsWhitespaceString()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IModerationsApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.CreateModerationAsync("   ", cancellationToken: CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CreateModerationAsync_RejectsAllWhitespaceArray()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IModerationsApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.CreateModerationAsync(new[] { " ", "" }, cancellationToken: CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetModelsAsync_UsesModelsEndpointAndMapsData()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{ \"object\": \"list\", \"data\": [ { \"id\": \"gpt-4o\", \"object\": \"model\", \"created\": 1700000000, \"owned_by\": \"openai\" } ] }"));
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IModelsApi>();
        var models = await api.GetModelsAsync(CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://api.openai.com/v1/models", request.RequestUri!.AbsoluteUri);

        Assert.Single(models);
        var m = models[0];
        Assert.Equal("gpt-4o", m.Id);
        Assert.Equal("openai", m.OwnedBy);
        Assert.Equal(1700000000L, m.Created);
    }

    [Fact]
    public async Task RetrieveModelAsync_EscapesIdAndUsesModelEndpoint()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{ \"id\": \"gpt 4\", \"object\": \"model\", \"created\": 1700000000, \"owned_by\": \"openai\" }"));
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IModelsApi>();
        var model = await api.RetrieveModelAsync("gpt 4", CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://api.openai.com/v1/models/gpt%204", request.RequestUri!.AbsoluteUri);
        Assert.Equal("gpt 4", model.Id);
    }

    [Fact]
    public async Task RetrieveModelAsync_RejectsBlankId()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IModelsApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.RetrieveModelAsync("  ", CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task ListFilesAsync_UsesFilesEndpointAndMapsData()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{ \"object\": \"list\", \"data\": [ { \"id\": \"file-1\", \"object\": \"file\", \"bytes\": 10, \"created_at\": 1, \"filename\": \"a.jsonl\", \"purpose\": \"fine-tune\", \"status\": \"processed\" } ] }"));
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IFilesApi>();
        var files = await api.ListFilesAsync(CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("https://api.openai.com/v1/files", request.RequestUri!.AbsoluteUri);

        Assert.Single(files);
        var f = files[0];
        Assert.Equal("file-1", f.Id);
        Assert.Equal("a.jsonl", f.Filename);
        Assert.Equal("processed", f.Status);
        Assert.Equal(10, f.Bytes);
    }

    [Fact]
    public async Task DeleteFileAsync_UsesDeleteMethodAndEscapesId()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{ \"id\": \"file 1\", \"object\": \"file\", \"deleted\": true }"));
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IFilesApi>();
        var resp = await api.DeleteFileAsync("file 1", CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Delete, request.Method);
        Assert.Equal("https://api.openai.com/v1/files/file%201", request.RequestUri!.AbsoluteUri);
        Assert.True(resp.Deleted);
        Assert.Equal("file 1", resp.Id);
    }

    [Fact]
    public async Task DeleteFileAsync_RejectsBlankId()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IFilesApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.DeleteFileAsync("  ", CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task CreateImageAsync_SendsExpectedRequest()
    {
        string? requestBody = null;
        var handler = new StubHttpMessageHandler(request =>
        {
            requestBody = ReadJsonBody(request);
            return JsonResponse("{ \"created\": 1, \"data\": [ { \"url\": \"https://example.com/a.png\", \"revised_prompt\": \"calmer\" } ], \"usage\": { \"total_tokens\": 10, \"input_tokens\": 6, \"output_tokens\": 4, \"input_tokens_details\": { \"text_tokens\": 2, \"image_tokens\": 4 } } }");
        });
        using var provider = CreateProvider(handler);

        var api = provider.GetRequiredService<IImagesApi>();
        var result = await api.CreateImageAsync(new CreateImageRequest { Prompt = "a green rabbit", Model = "gpt-image-2", N = 1, Size = "1024x1024" }, CancellationToken.None);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://api.openai.com/v1/images/generations", request.RequestUri!.AbsoluteUri);

        Assert.NotNull(requestBody);
        using var document = JsonDocument.Parse(requestBody);
        Assert.Equal("a green rabbit", document.RootElement.GetProperty("prompt").GetString());
        Assert.Equal("gpt-image-2", document.RootElement.GetProperty("model").GetString());
        Assert.Equal(1, document.RootElement.GetProperty("n").GetInt32());
        Assert.Equal("1024x1024", document.RootElement.GetProperty("size").GetString());

        Assert.Equal("https://example.com/a.png", result.Data[0].Url);
        Assert.Equal("calmer", result.Data[0].RevisedPrompt);
        Assert.Equal(10, result.Usage.TotalTokens);
    }

    [Fact]
    public async Task CreateImageAsync_RejectsBlankPrompt()
    {
        var handler = new StubHttpMessageHandler(_ => JsonResponse("{}"));
        using var provider = CreateProvider(handler);
        var api = provider.GetRequiredService<IImagesApi>();

        await Assert.ThrowsAsync<ArgumentException>(() => api.CreateImageAsync(new CreateImageRequest { Prompt = "  " }, CancellationToken.None));
        Assert.Empty(handler.Requests);
    }

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

    private static HttpResponseMessage JsonResponse(string json) => new(HttpStatusCode.OK)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };

    private static string ReadJsonBody(HttpRequestMessage request) => request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();

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
