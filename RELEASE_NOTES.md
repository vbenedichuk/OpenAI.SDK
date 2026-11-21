# Release notes

## 2.0.0

### Highlights

- `CancellationToken` support on every API method, exposed as an optional parameter.
- Modern Chat Completions DTOs: tools/function calling, structured output (`response_format`), multimodal content parts, `max_completion_tokens`, `seed`, `logprobs`.
- Configurable request timeout (`OpenAiOptions.Timeout`) that bounds the whole request, including reading the response body, and a named `HttpClient` (`"OpenAI"`) for Polly/retry/proxy integration.
- Input validation with clear `ArgumentException`/`ArgumentNullException` instead of NREs/remote 400s.
- Robust HTTP handling: JSON bodies are streamed when their length is known, so empty bodies (including chunked ones without `Content-Length`), error bodies and deserialization failures all behave predictably; error bodies are capped at 64 KB.
- Path segments are escaped and resources are confined to the configured base URL; `ConfigureAwait(false)` throughout.
- Richer `OpenAiApiException` (request URI, response headers, `Retry-After`, truncated body in the message), plus dedicated `OpenAiResponseFormatException` and `OpenAiTimeoutException`.
- Image generation/editing can target current GPT image models via `Model`/`model`.
- Centralized HTTP pipeline (JSON, raw bytes, multipart) eliminating drift between `FilesApi` and `ImagesApi`.
- Sample app no longer requires a debugger to read user secrets and fails fast with a clear message.
- README rewritten; retired endpoints clearly marked.

### Added

- `OpenAiOptions.Timeout` (default 100s, covers the whole request).
- `AddOpenAiApi` registers an `IStartupValidator`, so a host fails at startup when `OpenAiOptions` is invalid.
- `ConfigurationExtensions.HttpClientName` (`"OpenAI"`) constant.
- Chat DTOs: `ChatTool`, `ChatToolFunction`, `ChatToolCall`, `ChatToolCallFunction`, `ChatFunctionCall`, `ChatResponseFormat`.
- `ApiChatCompletionRequest`: `MaxCompletionTokens`, `ResponseFormat`, `Seed`, `Tools`, `ToolChoice`, `Functions`, `FunctionCall`, `Logprobs`, `TopLogprobs`.
- `ApiChatCompletionRequest.Stop` now accepts a string or an array.
- `ChatMessage`: `Name`, `ToolCalls`, `ToolCallId`, `FunctionCall`, `ContentText`; `Content` is now `object` (string or content-part array) and is read back as `string` for plain-text responses.
- `ApiChatCompletionResponse`: `Model`, `SystemFingerprint`, `ServiceTier`.
- `ChatCompletionChoice.Logprobs`.
- `CreateImageRequest`: `Model`, `Quality`, `OutputFormat`, `Background`.
- `IImagesApi.CreateImageEditAsync`: `model`, `quality`, `outputFormat` and `background` parameters; `maskFileName` defaults to `mask.png`.
- `OpenAiApiException.RequestUri`, `ResponseHeaders`, `RetryAfter`.
- `OpenAiResponseFormatException` (`StatusCode`, `ResponseContents`, `RequestUri`, original `JsonException` in `InnerException`) for 2xx bodies that cannot be deserialized.
- `OpenAiTimeoutException` (`RequestUri`, `Timeout`) for configured-timeout expiry.
- `FileStatusDetails` (`Code`, `Message`) and `FileDetails.Status`/`StatusDetails`.
- `ImageUsage`/`ImageInputTokensDetails` (`TotalTokens`, `InputTokens`, `OutputTokens`, `InputTokensDetails`); `CreateImageResponse.Usage`, `Size`, `Quality`, `OutputFormat`, `Background`; `CreateImageResponseData.RevisedPrompt`.
- `ModelUsage.PromptTokensDetails`/`CompletionTokensDetails` (cached, audio, reasoning, accepted/rejected prediction tokens); `ApiModel.Created`.
- Typed chat payloads: `ChatTokenLogprob`/`ChatTopLogprob`, `ChatAnnotation`/`ChatUrlCitation`, `ChatMessageAudio`; `ChatMessage.Refusal`.
- `BaseApi.ExecuteRawRequestAsync` and `BaseApi.ExecuteMultipartRequestAsync`.
- `FineTunesApi.ListFineTuneEventsAsync` throws `NotSupportedException` when `stream == true`.

### Changed

- `OpenAiOptions.BaseUrl` defaults to `https://api.openai.com/v1/`.
- `OpenAiOptions.ApiKey` is trimmed before it is sent; a blank key still fails validation.
- `OpenAiOptions.BaseUrl` with a query string or fragment is rejected instead of being silently ignored.
- `OpenAiOptions.Timeout` takes precedence over `HttpClient.Timeout`: the named client is created with `Timeout.InfiniteTimeSpan`, so a 100 s default no longer truncates longer configured timeouts.
- `OpenAiApiException` message now includes status code, request URI and a truncated (1024 characters) response body; `ResponseContents` is capped at 64 KB.
- `ChatMessage.Content` changed from `string` to `object`.
- `CreateImageEditAsync` signature: `model`, `quality`, `outputFormat` and `background` inserted before `CancellationToken`.
- `EmbeddingsRequest.Input` changed from `string[]` to `object` (string, string array, token array or token array array).
- `ApiCompletionRequest.Stop` changed from `string` to `object` (string or up to four strings).
- `ChatCompletionChoice.Logprobs` changed from `object` to `ChatCompletionLogprobs`; `ChatCompletionLogprobs.Content`/`Refusal` are `List<ChatTokenLogprob>`.
- `ChatMessage.Annotations` is `List<ChatAnnotation>` and `ChatMessage.Audio` is `ChatMessageAudio`.
- `FileDetails.StatusDetails` is `FileStatusDetails`; `CreateImageResponse.Usage` is `ImageUsage`.
- `FilesApi`/`ImagesApi` routed through the shared HTTP helpers.
- Validation in the multipart methods (`UploadFileAsync`, `CreateImageEditAsync`, `CreateImageVariationAsync`) throws synchronously, before the `Task` is returned.
- README "Supported APIs" split into supported vs. retired; `IModelsApi` documented.
- Sample uses `gpt-image-2`, `gpt-4o-mini`, `text-embedding-3-small`.
- Public member typos corrected (`Temperatue` → `Temperature`, `FimeTunedModel` → `FineTunedModel`, `Alow*` → `Allow*`, `PromptLossWeitghs` → `PromptLossWeight`).
- Retired APIs (`ICompletionsApi`, `IEditsApi`, `IFineTunesApi`, `IEnginesApi`) are marked `[Obsolete("API is not supported anymore")]`, and both image-variation methods are `[Obsolete]` in favour of `CreateImageEditAsync` with a GPT Image model.
- `OpenAiApiException`, `OpenAiResponseFormatException` and `OpenAiTimeoutException` implement the recommended `ISerializable` pattern (`GetObjectData`), so they survive serialization round-trips; `ResponseHeaders` is intentionally not serialized and `RetryAfter` is preserved instead.

### Fixed

- Sample documented `dotnet run` path no longer fails with an empty key / `AggregateException`.
- Missing `ApiKey`/invalid `BaseUrl` produce clear `ArgumentException` instead of NRE/`UriFormatException`.
- Null requests and missing required fields no longer send empty POSTs or NRE; null streams/arrays now throw `ArgumentNullException`.
- Successful responses with an empty body (`204 No Content`, `Content-Length: 0`, or chunked with no payload) return `default` instead of throwing `JsonException`.
- Malformed JSON now surfaces as `OpenAiResponseFormatException` with the captured response body and the original `JsonException` in `InnerException`; previously the error path failed with `InvalidOperationException: The stream was already consumed` or reported a 2xx status through `OpenAiApiException`.
- The configured timeout now also covers reading the response body (regression from `HttpCompletionOption.ResponseHeadersRead`) and surfaces as `OpenAiTimeoutException` instead of an ambiguous `OperationCanceledException`.
- Timeouts above the platform timer maximum no longer throw `ArgumentOutOfRangeException` from `CancelAfter`; the timer is clamped while the configured value is preserved in `OpenAiTimeoutException.Timeout`.
- Large non-2xx bodies are read only up to 64 KB instead of being fully buffered.
- `CreateImageEditAsync` no longer throws `ArgumentNullException` when a mask is supplied without `maskFileName`; the file name defaults to `mask.png`.
- The sample project is no longer packable, so `dotnet pack OpenAI.sln` does not produce a stray `OpenAI.SDK.SampleConsoleApp` package.
- Configuration validation is shared between runtime and startup (`OpenAiOptionsValidator`), so a host fails fast on a missing key or an unusable `BaseUrl`.
- The sample summarizes generated images (URL or base64 length) instead of dumping `b64_json`; the unused `BlackCircle.png` asset and its copy directive were removed.
- The sample now reports `OpenAiTimeoutException`/`OpenAiResponseFormatException` through `OpenAiException` (instead of letting them escape) and falls back to `OPENAI_API_KEY` when the user-secrets value is whitespace.
- `FineTunesApi.ListFineTuneEventsAsync` no longer silently ignores `stream`.
- `ModerationsApi` rejects null/empty input; the ambiguous `CreateModeration(null)` overload was removed.
- ID/path segments are escaped, `.`/`..` segments and resources escaping the base URL are rejected.
- `ConfigureAwait(false)` added to prevent sync-over-async deadlocks on .NET Framework.
- Build warnings and vulnerable transitive dependencies addressed.

### Breaking changes

1. `IImagesApi.CreateImageEditAsync` — `model`, `quality`, `outputFormat` and `background` parameters added before `CancellationToken`. Named arguments are unaffected; positional callers must update.
2. `ChatMessage.Content` changed from `string` to `object`. Plain-text responses still deserialize as `string`, so `msg.Content as string` and `msg.ContentText` keep working; multi-part responses are `JsonElement`.
3. `ApiChatCompletionRequest.Stop` changed from `string` to `object`.
4. `EmbeddingsRequest.Input` changed from `string[]` to `object`.
5. `ChatCompletionChoice.Logprobs` changed from `object` to `ChatCompletionLogprobs`; `ChatCompletionLogprobs.Content`/`Refusal` changed from `object` to `List<ChatTokenLogprob>`.
6. `ChatMessage.Annotations` changed from `object` to `List<ChatAnnotation>`; `ChatMessage.Audio` changed from `object` to `ChatMessageAudio`.
7. `FileDetails.StatusDetails` changed from `string` to `FileStatusDetails`; `CreateImageResponse.Usage` changed from `ModelUsage` to `ImageUsage`.
8. A 2xx body that cannot be deserialized now throws `OpenAiResponseFormatException` instead of `OpenAiApiException`; catch `OpenAiException` to keep handling both.
9. A configured timeout now throws `OpenAiTimeoutException` instead of `OperationCanceledException`; caller cancellation still throws `OperationCanceledException`/`TaskCanceledException`.
10. `FilesApi`/`ImagesApi`/`ChatApi`/etc. now throw `ArgumentException`/`ArgumentNullException` for missing required arguments where they previously sent an invalid request.
11. `OpenAiOptions.Timeout` overrides `HttpClient.Timeout`; a `client.Timeout` set through `AddHttpClient` no longer applies.
12. Methods renamed to the `Async` suffix: `IEditsApi.CreateEdit` → `CreateEditAsync`, `IEmbeddingsApi.CreateEmbeddings` → `CreateEmbeddingsAsync`, `IEnginesApi.RetrieveEngine` → `RetrieveEngineAsync`, `IModerationsApi.CreateModeration` → `CreateModerationAsync`, and `IFineTunesApi.ListFineTuneEvents` → `ListFineTuneEventsAsync`.
13. The `[Obsolete]` overloads without a `CancellationToken` were removed from every interface; each method now has a single signature with an optional `CancellationToken` parameter.
14. `ApiCreateEditRespoinse` was renamed to `ApiCreateEditResponse` (typo fix).
15. Public member typos fixed: `ApiModelPermission.AlowCreateEngine`/`AlowSampling` → `AllowCreateEngine`/`AllowSampling`, `Temperatue` → `Temperature` in `ApiChatCompletionRequest`, `ApiCompletionRequest` and `ApiCreateEditRequest`, `FineTune.FimeTunedModel` → `FineTunedModel`, and `PromptLossWeitghs` → `PromptLossWeight` in `CreateFineTuneRequest`/`FineTuneHyperparams`.

### Tests

- 95 tests, all passing.
- New coverage: configuration validation (missing key, invalid/non-HTTP/query/fragment base URL, startup validator, API key trimming), default base URL, image model serialization and `model`/`quality`/`output_format`/`background` propagation in multipart edits, mask filename defaulting, `BaseApi` error path with the 64 KB body cap, request shape (method, auth, content type, null omission), empty bodies with and without `Content-Length`, malformed JSON wrapping for JSON and multipart requests, `RequestUri`/`RetryAfter`/body truncation, URL escaping and path-traversal rejection, timeout enforcement during header wait and body read (including request context and oversized timeouts), cancellation propagation (deterministic, no sleeps), `ChatMessage.Content` string/multi-part round-trip, typed deserialization of file status details, image usage, logprobs, annotations, audio, refusal, token details and model/image metadata, request-shape/validation coverage for embeddings, moderations, models, file listing/deletion and image generation, raw file content download behavior (`Accept` header and `204` empty body), the sample's exception report for API, format and timeout failures, and `ISerializable` round-tripping of `OpenAiApiException`, `OpenAiResponseFormatException` and `OpenAiTimeoutException`.

### Commits

- `6748262` Fix sample configuration and add image model and options validation
- `a2cadee` Use current GPT image model in sample and docs
- `85d82bf` Add input validation, robust HTTP handling, ConfigureAwait, URL escaping, centralized requests, modern chat DTOs, and stronger tests
- `bb27863` Clean up cancellation tests and remove nullable warnings
- `c1cab14` Update README and add release notes
- `59637ce` Fix response handling, timeouts and URL containment after code review (buffer the response body once, apply the timeout to the whole request, confine resources to the base URL, read plain-text chat content as `string`, and close the related test gaps)
- `e34a68d` Describe review-driven hardening in release notes
- `03aa318` Fix review findings and refresh documentation
- `16b706d` Fix HTTP timeout and response handling, expand DTOs and image edit options, and update package metadata (typed file status/image usage/chat payloads, dedicated `OpenAiResponseFormatException`/`OpenAiTimeoutException`, 64 KB error-body cap, timeout request context, infinite-timeout named client, mask filename default)
- `0f786b2` Type remaining response payloads, add timeout context and regression tests, document breaking changes
- `5bf46a1` Report all SDK exceptions in the sample and add supported-API test coverage
- `09c5a00` Validate options at startup, trim API keys, reject BaseUrl query/fragment, and clean up sample/tests
- `9ef5691` Load sample configuration from environment and add coverage
- `58f2b15` Refactor BaseApi request pipeline and drop sample InternalsVisibleTo
- `3291213` Sync test count in docs
- `7dc56ec` Fix Sonar issues (remove obsolete no-token overloads and redundant XML docs)
- `9113505` Code cleanup
- `d54c077` Add CancellationToken to remaining request paths
- `b6321de` Update OpenAiTimeoutException `ISerializable` implementation to the recommended serialization pattern
- `3b38f3d` Add `ISerializable` support to `OpenAiResponseFormatException`
- `1d91b94` Documentation fixed; .NET version restored to netstandard2.0
- `252dcfd` Version 2.0.0 (merge)
