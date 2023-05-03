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
4. [Images](https://platform.openai.com/docs/api-reference/images). Implemented as IImagesAPi.
5. [Embeddings](https://platform.openai.com/docs/api-reference/embeddings). Implemented as IEmbeddingsApi.
6. [Files](https://platform.openai.com/docs/api-reference/files). Implemented as IFilesApi.
7. [Fine-tunes](https://platform.openai.com/docs/api-reference/fine-tunes). Implemented as IFineTunesApi.
8. [Moderations](https://platform.openai.com/docs/api-reference/moderations). Implemented as IModerationsApi.
9. [Engines](https://platform.openai.com/docs/api-reference/engines). Implemented as IEnginesApi.
10. [Audio] (https://platform.openai.com/docs/api-reference/audio). Implemented as IAudio.

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

4. Add Http Client
`
            builder.Services.AddHttpClient();
`

5. Add OpenAiApi to the application Service Collection.
`
            builder.Services.AddOpenAiApi();
`

6. Inject required services through constructors or obtain it using IServiceProvider when necessary.

7. Use it.


# Versions history
- 1.0. First release.
- 1.1. Chat api support added.
- 1.2. Code cleanup and renamings.
- 1.3. Embeddings api call fixed.
- 1.4. Audio api calls are added.