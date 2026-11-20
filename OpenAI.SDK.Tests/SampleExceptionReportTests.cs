using OpenAI.SDK.Exceptions;
using OpenAI.SDK.SampleConsoleApp;
using System;
using System.Net;
using System.Text.Json;

namespace OpenAI.SDK.Tests;

public class SampleExceptionReportTests
{
    [Fact]
    public void ExceptionReport_IncludesApiStatusAndBody()
    {
        var uri = new Uri("https://api.openai.com/v1/chat/completions");
        var exception = new OpenAiApiException(HttpStatusCode.BadRequest, "{\"error\":\"invalid\"}", "Bad Request", uri, null);
        var text = ExceptionReport.ToText(exception);
        Assert.Contains("400", text);
        Assert.Contains(uri.AbsoluteUri, text);
        Assert.Contains("{\"error\":\"invalid\"}", text);
    }

    [Fact]
    public void ExceptionReport_IncludesFormatFailureBody()
    {
        var exception = new OpenAiResponseFormatException(HttpStatusCode.OK, "{not-json", "Failed to deserialize response body.", new Uri("https://api.openai.com/v1/chat/completions"), new JsonException("bad json"));
        var text = ExceptionReport.ToText(exception);
        Assert.Contains("Failed to deserialize response body.", text);
        Assert.Contains("{not-json", text);
    }

    [Fact]
    public void ExceptionReport_IncludesTimeoutContext()
    {
        var uri = new Uri("https://api.openai.com/v1/images/generations");
        var exception = new OpenAiTimeoutException(uri, TimeSpan.FromSeconds(5), new OperationCanceledException());
        var text = ExceptionReport.ToText(exception);
        Assert.Contains(uri.AbsoluteUri, text);
        Assert.Contains("00:00:05", text);
    }
}