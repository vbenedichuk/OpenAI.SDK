# OpenAI.SDK

[![VB.OpenAI.SDK](https://img.shields.io/nuget/v/VB.OpenAI.SDK?style=for-the-badge)](https://www.nuget.org/packages/VB.OpenAI.SDK/)

A C# wrapper around the [OpenAI REST API](https://platform.openai.com/docs/api-reference), targeting `netstandard2.0` and published as `VB.OpenAI.SDK`.

## Features

- Chat completions (including tools, structured output, multimodal messages)
- Image generation and editing
- Embeddings, files, moderations, models
- Dependency-injection friendly (`Microsoft.Extensions.DependencyInjection`)
- `CancellationToken` support on every non-obsolete method
- Clear configuration validation and typed exceptions
- Retired endpoints kept for backward compatibility

## Requirements

- The library targets `netstandard2.0`, so it can be consumed from .NET Framework 4.6.1+, .NET Core 2.0+ and .NET 5+.
- Building the repository requires the .NET 10 SDK because the sample app targets `net10.0`. The test project targets `net8.0`.

## Installation

```bash
dotnet add package VB.OpenAI.SDK
```

## Configuration

Add the following to `appsettings.json` (or use user secrets / environment variables for the key):

```json
{
  "OpenAiOptions": {
    "ApiKey": "YOUR_KEY_HERE",
    "BaseUrl": "https://api.openai.com/v1/",
    "Timeout": "00:01:40"
  }
}
```

`BaseUrl` defaults to `https://api.openai.com/v1/` and `Timeout` defaults to 100 seconds when omitted. `ApiKey` is required; leading and trailing whitespace is trimmed. `BaseUrl` must be an absolute HTTP(S) URL without a query string or fragment. `AddOpenAiApi` registers startup validation, so a host fails at startup when the configuration is invalid. The section name is arbitrary — it only has to match the `Configure` call below. (The bundled sample app uses its own `OpenAI:Api` section, see [Sample application](#sample-application).)

Register the SDK in your DI container:

```csharp
using OpenAI.SDK.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(nameof(OpenAiOptions)));
builder.Services.AddOpenAiApi();
```

`AddOpenAiApi` registers a named `HttpClient` (`OpenAI.SDK.Configuration.ConfigurationExtensions.HttpClientName` == `"OpenAI"`) so you can attach Polly policies, proxies or custom handlers:

```csharp
builder.Services.AddHttpClient(OpenAI.SDK.Configuration.ConfigurationExtensions.HttpClientName)
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1)));
```

(The retry policy comes from the `Microsoft.Extensions.Http.Polly` package and is shown as an example — the SDK itself takes no dependency on Polly.)

`OpenAiOptions.Timeout` bounds the whole request, including reading the response body, and takes precedence over `HttpClient.Timeout`. Configure it through options rather than through `AddHttpClient(..., client => client.Timeout = ...)`:

```csharp
builder.Services.Configure<OpenAiOptions>(options => options.Timeout = TimeSpan.FromMinutes(5));
```

If `ApiKey` is missing or `BaseUrl` is invalid, resolving any API throws a clear `ArgumentException` instead of a cryptic `NullReferenceException`/`UriFormatException`.

## Supported APIs

Every API integration is exposed as a separate interface. Some upstream endpoints have been retired by OpenAI; the corresponding interfaces remain registered for backward compatibility and are marked `[Obsolete]`.

Currently supported:

1. [Chat](https://platform.openai.com/docs/api-reference/chat) — `IChatApi`
2. [Images](https://platform.openai.com/docs/api-reference/images) — `IImagesApi`
3. [Embeddings](https://platform.openai.com/docs/api-reference/embeddings) — `IEmbeddingsApi`
4. [Files](https://platform.openai.com/docs/api-reference/files) — `IFilesApi`
5. [Moderations](https://platform.openai.com/docs/api-reference/moderations) — `IModerationsApi`
6. [Models](https://platform.openai.com/docs/api-reference/models) — `IModelsApi`

Retired (kept for backward compatibility, marked `[Obsolete]`):

1. Text Completion — `ICompletionsApi`
2. Edits — `IEditsApi`
3. Fine-tunes — `IFineTunesApi`
4. Engines — `IEnginesApi`
5. Image variations — `IImagesApi.CreateImageVariation*`

## Usage

All non-obsolete API methods accept a `CancellationToken`. The legacy overloads without a token are marked `[Obsolete]` and delegate to the token-aware versions.

### Chat completions

```csharp
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Models.Chat;

public sealed class ChatService
{
    private readonly IChatApi _chatApi;

    public ChatService(IChatApi chatApi) => _chatApi = chatApi;

    public Task<ApiChatCompletionResponse> CompleteAsync(string message, CancellationToken cancellationToken)
    {
        return _chatApi.CreateCompletionAsync(new ApiChatCompletionRequest
        {
            Model = "gpt-4o-mini",
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = message }
            },
            ResponseFormat = new ChatResponseFormat { Type = "json_object" }
        }, cancellationToken);
    }
}
```

`ChatMessage.Content` accepts either a string or an array of content parts (for vision input). Response messages expose plain text as `string`, so `message.Content as string` and `message.ContentText` both work; multi-part content is returned as a `JsonElement` array. Tool calling is supported via `Tools`, `ToolChoice`, `ToolCalls` and `ToolCallId`.

### Image generation

```csharp
var imagesApi = serviceProvider.GetRequiredService<IImagesApi>();

var result = await imagesApi.CreateImageAsync(new CreateImageRequest
{
    Model = "gpt-image-2",   // specify explicitly for current models
    Prompt = "A green rabbit jumping through a burning circle.",
    N = 1,
    Size = "1024x1024"
}, CancellationToken.None);
```

For image edits, pass `model` and the optional `quality`, `outputFormat` and `background` settings:

```csharp
await imagesApi.CreateImageEditAsync(
    image, "img.png", "a prompt",
    model: "gpt-image-2", quality: "high", outputFormat: "png", background: "transparent",
    cancellationToken: CancellationToken.None);
```

When a mask is supplied without `maskFileName`, the SDK uses `mask.png`.

Set `Model` explicitly for current GPT image models. When omitted, the request falls back to legacy DALL·E 2 behaviour.

### Error handling

Non-2xx responses throw `OpenAiApiException`:

```csharp
try
{
    await chatApi.CreateCompletionAsync(request, cancellationToken);
}
catch (OpenAiApiException ex)
{
    Console.WriteLine($"{(int)ex.StatusCode} {ex.StatusCode}");
    Console.WriteLine(ex.RequestUri);
    Console.WriteLine(ex.ResponseContents);
    if (ex.RetryAfter.HasValue)
    {
        await Task.Delay(ex.RetryAfter.Value, cancellationToken);
    }
}
```

`OpenAiApiException` exposes `StatusCode`, `ResponseContents`, `RequestUri`, `ResponseHeaders` and `RetryAfter`. `ResponseContents` contains the raw response body or, for large error responses, its first 64 KB. Its `Message` contains the status code, request URI and a truncated (up to 1024 characters) response body. A successful response body that cannot be deserialized into the expected DTO throws `OpenAiResponseFormatException` (with the captured body prefix, status code and request URI) and the original `JsonException` in `InnerException`; transport failures surface as `HttpRequestException`, caller cancellation as `OperationCanceledException`/`TaskCanceledException`, and a configured request timeout as `OpenAiTimeoutException` (exposing `RequestUri` and `Timeout`). A successful response with an empty body (for example `204 No Content`) returns `default` instead of throwing.

### Cancellation

Every non-obsolete method accepts a `CancellationToken`. Pre-cancelled tokens are honoured and dynamic cancellation propagates to the underlying HTTP request. A configured `OpenAiOptions.Timeout` is combined with the caller token; when that timeout elapses, the SDK throws `OpenAiTimeoutException`.

### Streaming

Streaming (`stream: true`) is not supported by this SDK. Setting it throws `NotSupportedException`.

## Building from source

```bash
dotnet build OpenAI.sln -p:GeneratePackageOnBuild=false
dotnet test OpenAI.SDK.Tests/OpenAI.SDK.Tests.csproj
```

The test suite uses local HTTP stubs and never calls the OpenAI API. Running `dotnet build OpenAI.sln` without the property also produces the NuGet package (`GeneratePackageOnBuild=True`).

## Sample application

`OpenAI.SDK.SampleConsoleApp` demonstrates embeddings, models, chat and image generation.

1. Obtain an API key from OpenAI.
2. Configure the key (either option works):
   - User secrets: `dotnet user-secrets set "OpenAI:Api:ApiKey" "YOUR_KEY_HERE"` (run from `OpenAI.SDK.SampleConsoleApp`)
   - Environment variable: set `OPENAI_API_KEY`
3. Run: `dotnet run --project OpenAI.SDK.SampleConsoleApp`

You can also override the base URL with `OpenAI:Api:BaseUrl`. If the API key is missing, the sample fails fast with a clear `InvalidOperationException`.

## Release notes

The full changelog lives in `RELEASE_NOTES.md` in the repository (it is not packed into the NuGet package).

## Version history

- **2.0.0** - `CancellationToken` support on all API methods; obsolete overloads without a token; modern chat DTOs (tools, structured output, multimodal content, typed logprobs/annotations/audio, `max_completion_tokens`); configurable timeout (honoured end-to-end) and named `HttpClient`; input validation and startup validation; trimmed API key; `BaseUrl` without query/fragment; robust HTTP handling (streamed JSON, empty bodies, 64 KB error-body cap, URL escaping, `ConfigureAwait(false)`); dedicated `OpenAiResponseFormatException`/`OpenAiTimeoutException` with request context; richer DTOs (file status details, image usage, token details); image model/quality/output format/background for generation and editing; updated README.
- **1.4.0** — Fixed file content download, moderation results, fine-tune file deserialization, DI registrations, logprobs mapping, base URL normalization, and streaming validation.
- **1.3** — Embeddings API call fixed.
- **1.2** — Code cleanup and renamings.
- **1.1** — Chat API support added.
- **1.0** — First release.

## License

See `LICENSE`.