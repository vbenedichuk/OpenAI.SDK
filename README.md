# OpenAI.SDK
[![VB.OpenAI.SDK](https://img.shields.io/nuget/v/VB.OpenAI.SDK?style=for-the-badge)](https://www.nuget.org/packages/VB.OpenAI.SDK/)

## Introduction
This library provides openai api wrapper written in C#.
Most of the samples are available in OpenAI.SDK.SampleConsoleApp app Program.cs file.

## OpenAI.SDK.SampleConsoleApp configuration
1. Obtain API key from OpenAI website.
2. Initialize user secrets storage `dotnet user-secrets init`
3. Add API key to the user secrets storage `dotnet user-secrets set "OpenAI:Api:ApiKey" "YOU KEY HERE"`

## Supported APIs
Every api integration is provided as separate interface class.
Following APIs are supported:
1. [Text Completion.](https://platform.openai.com/docs/api-reference/completions). Implemented as ICompletionsApi
2. [Chat](https://platform.openai.com/docs/api-reference/chat). Implemented as IChatApi.
3. [Edits](https://platform.openai.com/docs/api-reference/edits). Implemented as IEditsApi.
4. [Images](https://platform.openai.com/docs/api-reference/images). Implemented as IImagesApi.
5. [Embeddings](https://platform.openai.com/docs/api-reference/embeddings). Implemented as IEmbeddingsApi.
6. [Files](https://platform.openai.com/docs/api-reference/files). Implemented as IFilesApi.
7. [Fine-tunes](https://platform.openai.com/docs/api-reference/fine-tunes). Implemented as IFineTunesApi.
8. [Moderations](https://platform.openai.com/docs/api-reference/moderations). Implemented as IModerationsApi.
9. [Engines](https://platform.openai.com/docs/api-reference/engines). Implemented as IEnginesApi.

## How to use
This section describes a basic usage scenario for ASP.NET Core Web Api projects.
The library can be applied to other project types too, but some details may be specific on per project type basis. It is out of scope of this document.
Feel free to ask questions in [issues section](https://github.com/vbenedichuk/OpenAI.SDK/issues) of the project.

1. Create a new ASP.NET Core Web Api project or open the existing one. 

2. Add following lines to the appsettings.json:
`  "OpenAiOptions": {
    "ApiKey": "SET KEY HERE",
    "BaseUrl": "https://api.openai.com/v1/"
  },`

3. Configure `OpenAiOptions`.
`            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection(nameof(OpenAiOptions)));
`

4. Add OpenAiApi to the application Service Collection.
`
            builder.Services.AddOpenAiApi();
`

5. Inject required services through constructors or obtain it using IServiceProvider when necessary.

6. Use it.

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