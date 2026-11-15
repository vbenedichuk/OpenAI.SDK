# OpenAI.SDK
[![VB.OpenAI.SDK](https://img.shields.io/nuget/v/VB.OpenAI.SDK?style=for-the-badge)](https://www.nuget.org/packages/VB.OpenAI.SDK/)

## Introduction
This library provides openai api wrapper written in C#.
Most of the samples are available in OpenAI.SDK.SampleConsoleApp app Program.cs file.

## OpenAI.SDK.SampleConsoleApp configuration
1. Obtain an API key from the OpenAI website.
2. Configure the key for the sample app (either option works):
   - User secrets: `dotnet user-secrets set "OpenAI:Api:ApiKey" "YOUR_KEY_HERE"` (run from `OpenAI.SDK.SampleConsoleApp`)
   - Environment variable: set `OPENAI_API_KEY`
3. Run the sample: `dotnet run --project OpenAI.SDK.SampleConsoleApp`

If the API key is missing, the sample fails fast with a clear `InvalidOperationException` explaining how to configure it. You can also override the base URL with `OpenAI:Api:BaseUrl`.

## Supported APIs
Every API integration is provided as a separate interface class. Some of the upstream endpoints below have been retired by OpenAI; the corresponding interfaces remain registered for backward compatibility and are marked `[Obsolete]`.

Currently supported:
1. [Chat](https://platform.openai.com/docs/api-reference/chat). Implemented as `IChatApi`.
2. [Images](https://platform.openai.com/docs/api-reference/images). Implemented as `IImagesApi`.
3. [Embeddings](https://platform.openai.com/docs/api-reference/embeddings). Implemented as `IEmbeddingsApi`.
4. [Files](https://platform.openai.com/docs/api-reference/files). Implemented as `IFilesApi`.
5. [Moderations](https://platform.openai.com/docs/api-reference/moderations). Implemented as `IModerationsApi`.
6. [Models](https://platform.openai.com/docs/api-reference/models). Implemented as `IModelsApi`.

Retired (kept for backward compatibility, marked `[Obsolete]`):
1. Text Completion — `ICompletionsApi`.
2. Edits — `IEditsApi`.
3. Fine-tunes — `IFineTunesApi`.
4. Engines — `IEnginesApi`.
5. Image variations — `IImagesApi.CreateImageVariation*`.

Notes:
- Streaming (`stream: true`) is not supported; setting it throws `NotSupportedException`.
- Non-2xx responses throw `OpenAiApiException` (status code, body, request URI, response headers, `Retry-After`).
- `OpenAiOptions.Timeout` (default 100s) controls the request timeout; the SDK uses a named `HttpClient` (`ConfigurationExtensions.HttpClientName` = `"OpenAI"`) so Polly/retry policies can be attached.
- `BaseUrl` defaults to `https://api.openai.com/v1/`; `ApiKey` is required.

## How to use
This section describes a basic usage scenario for ASP.NET Core Web Api projects.
The library can be applied to other project types too, but some details may be specific on a per-project basis.
Feel free to ask questions in the [issues section](https://github.com/vbenedichuk/OpenAI.SDK/issues).

1. Create a new ASP.NET Core Web Api project or open an existing one.

2. Add the following to `appsettings.json` (or use user secrets / environment variables for the key):
```json
"OpenAiOptions": {
  "ApiKey": "YOUR_KEY_HERE",
  "BaseUrl": "https://api.openai.com/v1/"
}
```

3. Configure `OpenAiOptions` and register the SDK:
```csharp
using OpenAI.SDK.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(nameof(OpenAiOptions)));
builder.Services.AddOpenAiApi();
```

`BaseUrl` defaults to `https://api.openai.com/v1/` when not set. `ApiKey` is required; if it is missing or `BaseUrl` is invalid, the SDK throws a clear `ArgumentException` when an API is resolved.

4. Inject required services through constructors or obtain them from `IServiceProvider` when necessary.

5. Use it. For image generation, set `Model` explicitly (for example `gpt-image-2`) on `CreateImageRequest`; for image edits with GPT image models, pass `model` to `CreateImageEditAsync`. Legacy DALL·E 2 behavior is still available when no model is specified.

All API methods accept a `CancellationToken`, so a request can be cancelled when the caller goes away (client disconnect, request timeout, application shutdown):

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
            Model = "gpt-3.5-turbo",
            Messages = new List<ChatMessage>
            {
                new ChatMessage { Role = "user", Content = message }
            }
        }, cancellationToken);
    }
}
```

> **Deprecated:** overloads that do not accept a `CancellationToken` are marked `[Obsolete]` since 2.0.0 and will be removed in a future major version. Use the overload that accepts a token — pass `CancellationToken.None` if you do not need cancellation.

## Versions history
- 2.0.0. `CancellationToken` support added to all API methods; overloads without a token are marked obsolete.
- 1.4.0. Fixed file content download, moderation results, fine-tune file deserialization, DI registrations, logprobs mapping, base URL normalization, and streaming validation.
- 1.3. Embeddings api call fixed.
- 1.2. Code cleanup and renamings.
- 1.1. Chat api support added
- 1.0. First release.