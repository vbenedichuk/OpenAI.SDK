using System.Text.Json;
using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.Files;
using OpenAI.SDK.Models.FineTunes;
using OpenAI.SDK.Models.CreateImage;
using OpenAI.SDK.Models.Moderations;
using OpenAI.SDK.Models.Chat;
namespace OpenAI.SDK.Tests;

public class ModelSerializationTests
{
    [Fact]
    public void ModerationResult_DeserializesAndExposesValues()
    {
        const string json = """
            {
              "categories": { "hate": false, "violence": true },
              "category_scores": { "hate": 0.01, "violence": 0.97 },
              "flagged": true
            }
            """;

        var result = JsonSerializer.Deserialize<ModerationResult>(json);

        Assert.NotNull(result);
        Assert.True(result.Flagged);
        Assert.True(result.Categories["violence"]);
        Assert.Equal(0.97, result.CategoryScores["violence"]);
    }

    [Fact]
    public void FineTune_DeserializesFileCollectionsAsFileDetails()
    {
        const string json = """
            {
              "id": "ft-1",
              "object": "fine-tune",
              "result_files": [ { "id": "file-1", "object": "file", "bytes": 10, "filename": "result.csv", "purpose": "fine-tune-results" } ],
              "training_files": [ { "id": "file-2", "object": "file", "bytes": 20, "filename": "train.jsonl", "purpose": "fine-tune" } ],
              "validation_files": []
            }
            """;

        var fineTune = JsonSerializer.Deserialize<FineTune>(json);

        Assert.NotNull(fineTune);
        var resultFile = Assert.Single(fineTune.ResultFiles);
        Assert.Equal("file-1", resultFile.Id);
        Assert.Equal("result.csv", resultFile.Filename);

        var trainingFile = Assert.Single(fineTune.TrainingFiles);
        Assert.Equal("train.jsonl", trainingFile.Filename);
    }

    [Fact]
    public void CompletionChoice_DeserializesLogprobsObject()
    {
        const string json = """
            {
              "text": "hi",
              "index": 0,
              "logprobs": {
                "tokens": [ "hi" ],
                "token_logprobs": [ -0.1 ],
                "top_logprobs": [ { "hi": -0.1 } ],
                "text_offset": [ 0 ]
              },
              "finish_reason": "stop"
            }
            """;

        var choice = JsonSerializer.Deserialize<CompletionChoice>(json);

        Assert.NotNull(choice);
        Assert.NotNull(choice.Logprobs);
        Assert.Equal(new[] { "hi" }, choice.Logprobs.Tokens);
        Assert.Equal(new double?[] { -0.1 }, choice.Logprobs.TokenLogprobs);
        Assert.Equal(new[] { 0 }, choice.Logprobs.TextOffset);
    }

    [Fact]
    public void ApiCompletionRequest_SerializesLogprobsPropertyName()
    {
        var request = new ApiCompletionRequest { Model = "davinci", Logprobs = 5 };

        var json = JsonSerializer.Serialize(request);

        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("logprobs", out var value), json);
        Assert.Equal(5, value.GetInt32());
    }

    [Fact]
    public void CreateImageRequest_SerializesModelAndNewFields()
    {
        var request = new OpenAI.SDK.Models.CreateImage.CreateImageRequest
        {
            Prompt = "a cat",
            Model = "gpt-image-2",
            Quality = "high",
            OutputFormat = "png",
            Background = "transparent"
        };

        var json = JsonSerializer.Serialize(request);

        using var document = JsonDocument.Parse(json);
        Assert.Equal("gpt-image-2", document.RootElement.GetProperty("model").GetString());
        Assert.Equal("high", document.RootElement.GetProperty("quality").GetString());
        Assert.Equal("png", document.RootElement.GetProperty("output_format").GetString());
        Assert.Equal("transparent", document.RootElement.GetProperty("background").GetString());
    }

    [Fact]
    public void FineTuneEvent_DeserializesMessage()
    {
        const string json = """
            {
              "object": "fine-tune-event",
              "created_at": 1,
              "level": "info",
              "message": "Created fine-tune: ft-1"
            }
            """;

        var fineTuneEvent = JsonSerializer.Deserialize<FineTuneEvent>(json);

        Assert.NotNull(fineTuneEvent);
        Assert.Equal("Created fine-tune: ft-1", fineTuneEvent.Message);
    }

    [Fact]
    public void ChatMessage_SerializesStringContentAsJsonString()
    {
        var message = new OpenAI.SDK.Models.Chat.ChatMessage { Role = "user", Content = "hi" };

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

        using var document = JsonDocument.Parse(json);
        Assert.Equal("hi", document.RootElement.GetProperty("content").GetString());
        Assert.Equal("user", document.RootElement.GetProperty("role").GetString());
        Assert.False(document.RootElement.TryGetProperty("name", out _));
    }

    [Fact]
    public void ChatMessage_SerializesContentPartArrayAsJsonArray()
    {
        var parts = new List<object>
        {
            new Dictionary<string, object> { ["type"] = "text", ["text"] = "hi" }
        };
        var message = new OpenAI.SDK.Models.Chat.ChatMessage { Role = "user", Content = parts };

        var json = JsonSerializer.Serialize(message);

        using var document = JsonDocument.Parse(json);
        var content = document.RootElement.GetProperty("content");
        Assert.Equal(JsonValueKind.Array, content.ValueKind);
        Assert.Equal("text", content[0].GetProperty("type").GetString());
    }

    [Fact]
    public void ChatMessage_RoundTripsStringContentThroughRequest()
    {
        const string json = """{ "role": "assistant", "content": "hi" }""";

        var message = JsonSerializer.Deserialize<OpenAI.SDK.Models.Chat.ChatMessage>(json);
        var serialized = JsonSerializer.Serialize(message);

        using var document = JsonDocument.Parse(serialized);
        Assert.Equal("hi", document.RootElement.GetProperty("content").GetString());
        Assert.False(document.RootElement.TryGetProperty("ContentText", out _));
    }

    [Fact]
    public void FileDetails_DeserializesStatusDetailsObject()
    {
        const string json = """
            {
              "id": "file-1",
              "object": "file",
              "bytes": 3,
              "created_at": 1,
              "filename": "test.txt",
              "purpose": "fine-tune",
              "status": "error",
              "status_details": { "code": "invalid_format", "message": "Training file is not valid JSONL" }
            }
            """;

        var fileDetails = JsonSerializer.Deserialize<FileDetails>(json);

        Assert.NotNull(fileDetails);
        Assert.Equal("file-1", fileDetails.Id);
        Assert.Equal("error", fileDetails.Status);
        Assert.NotNull(fileDetails.StatusDetails);
        Assert.Equal("invalid_format", fileDetails.StatusDetails.Code);
        Assert.Equal("Training file is not valid JSONL", fileDetails.StatusDetails.Message);
    }

    [Fact]
    public void CreateImageResponse_DeserializesImageUsage()
    {
        const string json = """
            {
              "created": 1,
              "data": [],
              "usage": { "total_tokens": 100, "input_tokens": 60, "output_tokens": 40, "input_tokens_details": { "text_tokens": 10, "image_tokens": 50 } }
            }
            """;

        var response = JsonSerializer.Deserialize<CreateImageResponse>(json);

        Assert.NotNull(response);
        Assert.Equal(100, response.Usage.TotalTokens);
        Assert.Equal(60, response.Usage.InputTokens);
        Assert.Equal(40, response.Usage.OutputTokens);
        Assert.Equal(10, response.Usage.InputTokensDetails.TextTokens);
        Assert.Equal(50, response.Usage.InputTokensDetails.ImageTokens);
    }

    [Fact]
    public void ChatCompletionChoice_DeserializesTypedLogprobs()
    {
        const string json = """
            { "index": 0, "message": { "role": "assistant", "content": "hi" }, "finish_reason": "stop", "logprobs": { "content": [ { "token": "hi", "logprob": -0.25, "bytes": [104, 105], "top_logprobs": [ { "token": "hi", "logprob": -0.25, "bytes": [104, 105] } ] } ], "refusal": null } }
            """;

        var choice = JsonSerializer.Deserialize<ChatCompletionChoice>(json);

        Assert.NotNull(choice);
        Assert.NotNull(choice.Logprobs);
        Assert.Single(choice.Logprobs.Content);
        var entry = choice.Logprobs.Content[0];
        Assert.Equal("hi", entry.Token);
        Assert.Equal(-0.25, entry.Logprob);
        Assert.Equal(new[] { 104, 105 }, entry.Bytes);
        Assert.Single(entry.TopLogprobs);
        Assert.Equal("hi", entry.TopLogprobs[0].Token);
        Assert.Equal(-0.25, entry.TopLogprobs[0].Logprob);
    }

    [Fact]
    public void ChatMessage_DeserializesTypedAnnotations()
    {
        const string json = """
            { "role": "assistant", "content": "see source", "annotations": [ { "type": "url_citation", "url_citation": { "start_index": 0, "end_index": 10, "title": "Example", "url": "https://example.com" } } ] }
            """;

        var message = JsonSerializer.Deserialize<ChatMessage>(json);

        Assert.NotNull(message);
        Assert.Single(message.Annotations);
        var annotation = message.Annotations[0];
        Assert.Equal("url_citation", annotation.Type);
        Assert.Equal("Example", annotation.UrlCitation.Title);
        Assert.Equal("https://example.com", annotation.UrlCitation.Url);
        Assert.Equal(0, annotation.UrlCitation.StartIndex);
        Assert.Equal(10, annotation.UrlCitation.EndIndex);
    }

    [Fact]
    public void ChatMessage_DeserializesTypedAudio()
    {
        const string json = """
            { "role": "assistant", "content": "spoken", "audio": { "id": "audio_1", "expires_at": 1234567890, "data": "QUJD", "transcript": "spoken text" } }
            """;

        var message = JsonSerializer.Deserialize<ChatMessage>(json);

        Assert.NotNull(message);
        Assert.Equal("audio_1", message.Audio.Id);
        Assert.Equal(1234567890L, message.Audio.ExpiresAt);
        Assert.Equal("QUJD", message.Audio.Data);
        Assert.Equal("spoken text", message.Audio.Transcript);
    }

    [Fact]
    public void CreateImageResponseData_DeserializesRevisedPrompt()
    {
        const string json = """{ "url": "https://example.com/a.png", "revised_prompt": "a calmer version" }""";

        var data = JsonSerializer.Deserialize<CreateImageResponseData>(json);

        Assert.NotNull(data);
        Assert.Equal("https://example.com/a.png", data.Url);
        Assert.Equal("a calmer version", data.RevisedPrompt);
    }

    [Fact]
    public void ApiModel_DeserializesCreated()
    {
        const string json = """{ "id": "gpt-4o", "object": "model", "created": 1700000000, "owned_by": "openai" }""";

        var model = JsonSerializer.Deserialize<ApiModel>(json);

        Assert.NotNull(model);
        Assert.Equal("gpt-4o", model.Id);
        Assert.Equal("openai", model.OwnedBy);
        Assert.Equal(1700000000L, model.Created);
    }

    [Fact]
    public void ModelUsage_DeserializesTokenDetails()
    {
        const string json = """{ "prompt_tokens": 10, "completion_tokens": 20, "total_tokens": 30, "prompt_tokens_details": { "cached_tokens": 4, "audio_tokens": 2 }, "completion_tokens_details": { "reasoning_tokens": 7, "audio_tokens": 1 } }""";

        var usage = JsonSerializer.Deserialize<ModelUsage>(json);

        Assert.NotNull(usage);
        Assert.Equal(4, usage.PromptTokensDetails.CachedTokens);
        Assert.Equal(2, usage.PromptTokensDetails.AudioTokens);
        Assert.Equal(7, usage.CompletionTokensDetails.ReasoningTokens);
        Assert.Equal(1, usage.CompletionTokensDetails.AudioTokens);
    }

    [Fact]
    public void ChatMessage_DeserializesRefusal()
    {
        const string json = """{ "role": "assistant", "content": null, "refusal": "I cannot help with that." }""";

        var message = JsonSerializer.Deserialize<ChatMessage>(json);

        Assert.NotNull(message);
        Assert.Equal("I cannot help with that.", message.Refusal);
    }
}
