# Release notes

## 2.0.0

### Highlights

- `CancellationToken` support on every API method; overloads without a token are marked `[Obsolete]`.
- Modern Chat Completions DTOs: tools/function calling, structured output (`response_format`), multimodal content parts, `max_completion_tokens`, `seed`, `logprobs`.
- Configurable request timeout (`OpenAiOptions.Timeout`) that bounds the whole request, including reading the response body, and a named `HttpClient` (`"OpenAI"`) for Polly/retry/proxy integration.
- Input validation with clear `ArgumentException`/`ArgumentNullException` instead of NREs/remote 400s.
- Robust HTTP handling: the response body is buffered once, so empty bodies (including chunked ones without `Content-Length`), error bodies and deserialization failures all behave predictably.
- Path segments are escaped and resources are confined to the configured base URL; `ConfigureAwait(false)` throughout.
- Richer `OpenAiApiException` (request URI, response headers, `Retry-After`, truncated body in the message).
- Image generation/editing can target current GPT image models via `Model`/`model`.
- Centralized HTTP pipeline (JSON, raw bytes, multipart) eliminating drift between `FilesApi` and `ImagesApi`.
- Sample app no longer requires a debugger to read user secrets and fails fast with a clear message.
- README rewritten; retired endpoints clearly marked.

### Added

- `OpenAiOptions.Timeout` (default 100s, covers the whole request).
- `ConfigurationExtensions.HttpClientName` (`"OpenAI"`) constant.
- Chat DTOs: `ChatTool`, `ChatToolFunction`, `ChatToolCall`, `ChatToolCallFunction`, `ChatFunctionCall`, `ChatResponseFormat`.
- `ApiChatCompletionRequest`: `MaxCompletionTokens`, `ResponseFormat`, `Seed`, `Tools`, `ToolChoice`, `Functions`, `FunctionCall`, `Logprobs`, `TopLogprobs`.
- `ApiChatCompletionRequest.Stop` now accepts a string or an array.
- `ChatMessage`: `Name`, `ToolCalls`, `ToolCallId`, `FunctionCall`, `ContentText`; `Content` is now `object` (string or content-part array) and is read back as `string` for plain-text responses.
- `ApiChatCompletionResponse`: `Model`, `SystemFingerprint`, `ServiceTier`.
- `ChatCompletionChoice.Logprobs`.
- `CreateImageRequest`: `Model`, `Quality`, `OutputFormat`, `Background`.
- `IImagesApi.CreateImageEditAsync`: `model` parameter.
- `OpenAiApiException.RequestUri`, `ResponseHeaders`, `RetryAfter`.
- `BaseApi.ExecuteRawRequestAsync` and `BaseApi.ExecuteMultipartRequestAsync`.
- `FineTunesApi.ListFineTuneEventsAsync` throws `NotSupportedException` when `stream == true`.

### Changed

- `OpenAiOptions.BaseUrl` defaults to `https://api.openai.com/v1/`.
- `OpenAiOptions.Timeout` takes precedence over `HttpClient.Timeout`; configure the limit through options.
- `OpenAiApiException` message now includes status code, request URI and a truncated (1024 characters) response body; the reason phrase is no longer duplicated.
- `ChatMessage.Content` changed from `string` to `object`.
- `CreateImageEditAsync` signature: `model` inserted before `CancellationToken`.
- `FilesApi`/`ImagesApi` routed through the shared HTTP helpers.
- Validation in the multipart methods (`UploadFileAsync`, `CreateImageEditAsync`, `CreateImageVariationAsync`) throws synchronously, before the `Task` is returned.
- README "Supported APIs" split into supported vs. retired; `IModelsApi` documented.
- Sample uses `gpt-image-2`, `gpt-4o-mini`, `text-embedding-3-small`.

### Fixed

- Sample documented `dotnet run` path no longer fails with an empty key / `AggregateException`.
- Missing `ApiKey`/invalid `BaseUrl` produce clear `ArgumentException` instead of NRE/`UriFormatException`.
- Null requests and missing required fields no longer send empty POSTs or NRE; null streams/arrays now throw `ArgumentNullException`.
- Successful responses with an empty body (`204 No Content`, `Content-Length: 0`, or chunked with no payload) return `default` instead of throwing `JsonException`.
- Malformed JSON now surfaces as `OpenAiApiException` with the response body and the original `JsonException` in `InnerException`; previously the error path failed with `InvalidOperationException: The stream was already consumed`.
- The configured timeout now also covers reading the response body (regression from `HttpCompletionOption.ResponseHeadersRead`).
- `FineTunesApi.ListFineTuneEventsAsync` no longer silently ignores `stream`.
- `ModerationsApi` rejects null/empty input and the ambiguous `CreateModeration(null)` overload.
- ID/path segments are escaped, `.`/`..` segments and resources escaping the base URL are rejected.
- `ConfigureAwait(false)` added to prevent sync-over-async deadlocks on .NET Framework.
- Build warnings and vulnerable transitive dependencies addressed.

### Breaking changes

1. `IImagesApi.CreateImageEditAsync` — `model` parameter added before `CancellationToken`. Named arguments are unaffected; positional callers passing a token as the 10th argument must update.
2. `ChatMessage.Content` changed from `string` to `object`. Plain-text responses still deserialize as `string`, so `msg.Content as string` and `msg.ContentText` keep working; multi-part responses are `JsonElement`.
3. `ApiChatCompletionRequest.Stop` changed from `string` to `object`.
4. `FilesApi`/`ImagesApi`/`ChatApi`/etc. now throw `ArgumentException`/`ArgumentNullException` for missing required arguments where they previously sent an invalid request.
5. `OpenAiOptions.Timeout` overrides `HttpClient.Timeout`; a `client.Timeout` set through `AddHttpClient` no longer applies.

### Tests

- 44 tests, all passing.
- New coverage: configuration validation (missing key, invalid/non-HTTP base URL), default base URL, image model serialization and `model` propagation in multipart edits, `BaseApi` error path, request shape (method, auth, content type, null omission), empty bodies with and without `Content-Length`, malformed JSON wrapping, `RequestUri`/`RetryAfter`/body truncation, URL escaping and path-traversal rejection, timeout enforcement, cancellation propagation (deterministic, no sleeps), and `ChatMessage.Content` string/multi-part round-trip.

### Commits

- `6748262` Fix sample configuration and add image model and options validation
- `a2cadee` Use current GPT image model in sample and docs
- `85d82bf` Add input validation, robust HTTP handling, ConfigureAwait, URL escaping, centralized requests, modern chat DTOs, and stronger tests
- `bb27863` Clean up cancellation tests and remove nullable warnings
- `c1cab14` Update README and add release notes
- A review-driven hardening pass: buffer the response body once, apply the timeout to the whole request, confine resources to the base URL, read plain-text chat content as `string`, and close the related test gaps.
