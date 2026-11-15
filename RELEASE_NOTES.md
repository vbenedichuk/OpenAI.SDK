# Release notes

## 2.0.0

### Highlights

- `CancellationToken` support on every API method; overloads without a token are marked `[Obsolete]`.
- Modern Chat Completions DTOs: tools/function calling, structured output (`response_format`), multimodal content parts, `max_completion_tokens`, `seed`, `logprobs`.
- Configurable request timeout and a named `HttpClient` (`"OpenAI"`) for Polly/retry/proxy integration.
- Input validation with clear `ArgumentException`/`ArgumentNullException` instead of NREs/remote 400s.
- Robust HTTP handling: empty/204 responses no longer throw `JsonException`; URL segments are escaped; `ConfigureAwait(false)` throughout.
- Richer `OpenAiApiException` (request URI, response headers, `Retry-After`, truncated body in the message).
- Image generation/editing can target current GPT image models via `Model`/`model`.
- Centralized HTTP pipeline (JSON, raw bytes, multipart) eliminating drift between `FilesApi` and `ImagesApi`.
- Sample app no longer requires a debugger to read user secrets and fails fast with a clear message.
- README rewritten; retired endpoints clearly marked.

### Added

- `OpenAiOptions.Timeout` (default 100s).
- `ConfigurationExtensions.HttpClientName` (`"OpenAI"`) constant.
- Chat DTOs: `ChatTool`, `ChatToolFunction`, `ChatToolCall`, `ChatToolCallFunction`, `ChatFunctionCall`, `ChatResponseFormat`.
- `ApiChatCompletionRequest`: `MaxCompletionTokens`, `ResponseFormat`, `Seed`, `Tools`, `ToolChoice`, `Functions`, `FunctionCall`, `Logprobs`, `TopLogprobs`.
- `ApiChatCompletionRequest.Stop` now accepts a string or an array.
- `ChatMessage`: `Name`, `ToolCalls`, `ToolCallId`, `FunctionCall`; `Content` is now `object` (string or content-part array).
- `ApiChatCompletionResponse`: `Model`, `SystemFingerprint`, `ServiceTier`.
- `ChatCompletionChoice.Logprobs`.
- `CreateImageRequest`: `Model`, `Quality`, `OutputFormat`, `Background`.
- `IImagesApi.CreateImageEditAsync`: `model` parameter.
- `OpenAiApiException.RequestUri`, `ResponseHeaders`, `RetryAfter`.
- `BaseApi.ExecuteRawRequestAsync` and `BaseApi.ExecuteMultipartRequestAsync`.
- `FineTunesApi.ListFineTuneEventsAsync` throws `NotSupportedException` when `stream == true`.

### Changed

- `OpenAiOptions.BaseUrl` defaults to `https://api.openai.com/v1/`.
- `OpenAiApiException` message now includes status code, request URI, reason phrase and a truncated response body.
- `ChatMessage.Content` changed from `string` to `object`.
- `CreateImageEditAsync` signature: `model` inserted before `CancellationToken`.
- `FilesApi`/`ImagesApi` routed through the shared HTTP helpers.
- README "Supported APIs" split into supported vs. retired; `IModelsApi` documented.
- Sample uses `gpt-image-2`, `gpt-4o-mini`, `text-embedding-3-small`.

### Fixed

- Sample documented `dotnet run` path no longer fails with an empty key / `AggregateException`.
- Missing `ApiKey`/invalid `BaseUrl` produce clear `ArgumentException` instead of NRE/`UriFormatException`.
- Null requests and missing required fields no longer send empty POSTs or NRE.
- Empty/204 responses no longer throw `JsonException`.
- `FineTunesApi.ListFineTuneEventsAsync` no longer silently ignores `stream`.
- `ModerationsApi` rejects null/empty input and the ambiguous `CreateModeration(null)` overload.
- ID/path segments are escaped; absolute resource URIs rejected.
- `ConfigureAwait(false)` added to prevent sync-over-async deadlocks on .NET Framework.
- Build warnings and vulnerable transitive dependencies addressed.

### Breaking changes

1. `IImagesApi.CreateImageEditAsync` — `model` parameter added before `CancellationToken`. Named arguments are unaffected; positional callers passing a token as the 10th argument must update.
2. `ChatMessage.Content` changed from `string` to `object`. Code reading it as `string` must cast.
3. `ApiChatCompletionRequest.Stop` changed from `string` to `object`.
4. `FilesApi`/`ImagesApi`/`ChatApi`/etc. now throw `ArgumentException`/`ArgumentNullException` for missing required arguments where they previously sent an invalid request.

### Tests

- 31 tests, all passing.
- New coverage: configuration validation, default base URL, image model serialization/edit propagation, `BaseApi` error path, request shape (method, auth, content type, null omission), empty-body handling, cancellation propagation.

### Commits

- `6748262` Fix sample configuration and add image model and options validation
- `a2cadee` Use current GPT image model in sample and docs
- `85d82bf` Add input validation, robust HTTP handling, ConfigureAwait, URL escaping, centralized requests, modern chat DTOs, and stronger tests
- `bb27863` Clean up cancellation tests and remove nullable warnings