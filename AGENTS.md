# AGENTS.md

## Layout
- `OpenAI.SDK/` — netstandard2.0 library, published as `VB.OpenAI.SDK`. Public contracts live in `Abstractions/I*Api.cs`, implementations in `Logic/*Api.cs`, DTOs in `Models/`, DI wiring in `Configuration/ConfigurationExtensions.cs`.
- `OpenAI.SDK.Tests/` — xUnit on net8.0. Tests use local HTTP stubs and never call OpenAI.
- `OpenAI.SDK.SampleConsoleApp/` — net10.0 console demo.
- `README.md` — package-facing documentation packed into the NuGet package.
- `RELEASE_NOTES.md` — full changelog (not packed).
- `OpenAI.sln` — solution containing all three projects.

.NET SDK 10.x is required because the sample targets net10.0.

## Build and test
- `dotnet build OpenAI.sln` — compiles and packs the NuGet package (`GeneratePackageOnBuild=True`).
- `dotnet build OpenAI.sln -p:GeneratePackageOnBuild=false` — compile only; useful for quick checks.
- `dotnet test OpenAI.SDK.Tests/OpenAI.SDK.Tests.csproj` — all tests (92 at the time of writing).
- Single test: `dotnet test OpenAI.SDK.Tests/OpenAI.SDK.Tests.csproj --filter "FullyQualifiedName~TestName"`.
- There is no CI, lint, formatter, or analyzer config; build + tests are the verification bar.

## Conventions that bite
- The library project pins `LangVersion 8.0`. Do not use C# 9+ features (records, `init`, file-scoped namespaces, raw string literals) in `OpenAI.SDK/`. Tests and the sample use newer defaults.
- `OpenAiOptionsValidator` is the single source of configuration validation: it rejects a missing/blank `ApiKey` and a `BaseUrl` with a query string or fragment, and returns the normalized base URI. `BaseApi` and the `IStartupValidator` registered by `AddOpenAiApi` both use it; `ApiKey` is trimmed before it is sent.
- New API: define `IXxxApi` in `Abstractions/`, implement `XxxApi : BaseApi` in `Logic/`, register it in `AddOpenAiApi`, and add DTOs in `Models/` with `[JsonPropertyName]`.
- Follow the existing pattern: every method has a `CancellationToken` overload; the tokenless overload is `[Obsolete]` and delegates to it.
- All HTTP goes through `BaseApi`: `ExecuteRequest` (JSON), `ExecuteRawRequestAsync` (bytes) and `ExecuteMultipartRequestAsync` (form data). `BaseApi` normalizes `BaseUrl` (trims trailing `/` then appends `/`), sends Bearer `ApiKey`, ignores null JSON properties, validates/escapes resource paths, throws `OpenAiApiException` on non-2xx, `OpenAiResponseFormatException` when a 2xx body cannot be deserialized, and `OpenAiTimeoutException` when the configured timeout elapses.
- Streaming is unsupported. `ChatApi`, `CompletionsApi` and `FineTunesApi.ListFineTuneEventsAsync` throw `NotSupportedException` when the stream flag is set; tests assert the first two.
- Multipart methods wrap caller streams in `NonDisposingStream`; never dispose caller-owned streams.

## Retired APIs — do not "clean up" until the next major version update
- `ICompletionsApi`, `IEditsApi`, `IFineTunesApi`, `IEnginesApi`, and `IImagesApi.CreateImageVariation*` are `[Obsolete]` because OpenAI shut down the endpoints/models. They remain registered in DI and tested with local `#pragma` suppressions for backward compatibility.
- Keep those registrations and pragmas unless the next major version update is requested. `README.md`'s "Supported APIs" list matches the code; keep it in sync when adding, retiring or removing an API.

## Packaging
- `PackageReadmeFile=README.md` and `PackageLicenseFile=LICENSE`. Both are packed from the repo root via `..\README.md` and `..\LICENSE`; dropping either `<None Include>` makes `dotnet build` fail with NU5030.
- Keep `System.Text.Json` at 8.0.6+ (CVE-2024-43485) and `xunit` at 2.9.3+ (older versions pull vulnerable NETStandard.Library transitives).

## Sample gotcha
- `Program.cs` reads `OpenAI:Api:ApiKey` from user secrets, falls back to the `OPENAI_API_KEY` environment variable when that value is null or whitespace, and throws `InvalidOperationException` with setup instructions when neither is set. It also honours `OpenAI:Api:BaseUrl`.
- The sample catches `OpenAiException` (the SDK base type), so `OpenAiApiException`, `OpenAiResponseFormatException` and `OpenAiTimeoutException` are all reported without an unhandled stack trace.
- `ExceptionReport.cs` is compiled into both the sample and the test project through a linked `<Compile>` item, so the sample's error formatting stays unit-tested. Keep it dependency-free.
- Image generation output is summarized (URL or base64 length) instead of serializing the whole response, so `b64_json` never floods the console.
