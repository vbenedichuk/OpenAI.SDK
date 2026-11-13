using System.Text.Json;
using OpenAI.SDK.Models.Completions;
using OpenAI.SDK.Models.FineTunes;
using OpenAI.SDK.Models.Moderations;

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
}
