# OpenAI.SDK

[![VB.OpenAI.SDK](https://img.shields.io/nuget/v/VB.OpenAI.SDK?style=for-the-badge)](https://www.nuget.org/packages/VB.OpenAI.SDK/)

A C# wrapper around the [OpenAI REST API](https://platform.openai.com/docs/api-reference), targeting `netstandard2.0` and published as `VB.OpenAI.SDK`.

## Features

- Chat completions (including tools, structured output, multimodal messages)
- Image generation and editing
- Embeddings, files, moderations, models
- Dependency-injection friendly (`Microsoft.Extensions.DependencyInjection`)
- `CancellationToken` support on every method
- Clear configuration validation and typed exceptions
- Retired endpoints kept for backward compatibility

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

`BaseUrl` defaults to `https://api.openai.com/v1/` and `Timeout` defaults to 100 seconds when omitted. `ApiKey` is required.

Register the SDK in your DI container:

```csharp
using OpenAI.SDK.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(nameof(OpenAiOptions)));
builder.Services.AddOpenAiApi();
```

`AddOpenAiApi` registers a named `HttpClient` (`OpenAI.SDK.Configuration.ConfigurationExtensions.HttpClientName` == `"OpenAI"`) so you can attach Polly policies, proxies or custom handlers:

```csharp
builder.Services.AddHttpClient(OpenAI.SDK.Configuration.ConfigurationExtensions.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
});
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

All API methods accept a `CancellationToken`.

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

`ChatMessage.Content` accepts either a string or an array of content parts (for vision input). Tool calling is supported via `Tools`, `ToolChoice`, `ToolCalls` and `ToolCallId`.

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

For image edits, pass `model` to `CreateImageEditAsync`:

```csharp
await imagesApi.CreateImageEditAsync(image, "img.png", "a prompt", model: "gpt-image-2", cancellationToken: CancellationToken.None);
```

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

`OpenAiApiException` exposes `StatusCode`, `ResponseContents`, `RequestUri`, `ResponseHeaders` and `RetryAfter`. Network/JSON failures surface as `HttpRequestException`/`JsonException`/`OperationCanceledException`.

### Cancellation

Every method accepts a `CancellationToken`. Pre-cancelled tokens are honoured and dynamic cancellation propagates to the underlying HTTP request.

### Streaming

Streaming (`stream: true`) is not supported by this SDK. Setting it throws `NotSupportedException`.

## Sample application

`OpenAI.SDK.SampleConsoleApp` demonstrates embeddings, models, chat and image generation.

1. Obtain an API key from OpenAI.
2. Configure the key (either option works):
   - User secrets: `dotnet user-secrets set "OpenAI:Api:ApiKey" "YOUR_KEY_HERE"` (run from `OpenAI.SDK.SampleConsoleApp`)
   - Environment variable: set `OPENAI_API_KEY`
3. Run: `dotnet run --project OpenAI.SDK.SampleConsoleApp`

You can also override the base URL with `OpenAI:Api:BaseUrl`. If the API key is missing, the sample fails fast with a clear `InvalidOperationException`.

## Release notes

See [RELEASE_NOTES.md](RELEASE_NOTES.md) for the full changelog.

## Version history

- **2.0.0** — `CancellationToken` support on all API methods; obsolete overloads without a token; modern chat DTOs (tools, structured output, multimodal content, `max_completion_tokens`); configurable timeout and named `HttpClient`; input validation; robust HTTP handling (empty bodies, URL escaping, `ConfigureAwait(false)`); improved exception details; image model selection; updated README.
- **1.4.0** — Fixed file content download, moderation results, fine-tune file deserialization, DI registrations, logprobs mapping, base URL normalization, and streaming validation.
- **1.3** — Embeddings API call fixed.
- **1.2** — Code cleanup and renamings.
- **1.1** — Chat API support added.
- **1.0** — First release.

## License

See `LICENSE`.