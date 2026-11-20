using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using OpenAI.SDK.Exceptions;

#pragma warning disable SYSLIB0050 // Formatter-based serialization is exercised directly to verify the ISerializable contract.

namespace OpenAI.SDK.Tests;

public class ExceptionSerializationTests
{
    [Fact]
    public void OpenAiTimeoutException_SerializesRequestUriAndTimeout()
    {
        var requestUri = new Uri("https://api.openai.com/v1/chat/completions");
        var timeout = TimeSpan.FromSeconds(42);
        var exception = new OpenAiTimeoutException(requestUri, timeout, new OperationCanceledException());

        var info = CaptureObjectData(exception);
        var restored = CreateFromSerializationInfo<OpenAiTimeoutException>(info);

        Assert.Equal(requestUri, restored.RequestUri);
        Assert.Equal(timeout, restored.Timeout);
    }

    [Fact]
    public void OpenAiApiException_SerializesStatusCodeResponseContentsRequestUriAndRetryAfter()
    {
        var requestUri = new Uri("https://api.openai.com/v1/chat/completions");
        var headers = new HttpResponseMessage().Headers;
        headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7));
        var exception = new OpenAiApiException(
            HttpStatusCode.TooManyRequests,
            "{\"error\":\"rate limited\"}",
            "Too Many Requests",
            requestUri,
            headers);

        var info = CaptureObjectData(exception);
        var restored = CreateFromSerializationInfo<OpenAiApiException>(info);

        Assert.Equal(HttpStatusCode.TooManyRequests, restored.StatusCode);
        Assert.Equal("{\"error\":\"rate limited\"}", restored.ResponseContents);
        Assert.Equal(requestUri, restored.RequestUri);
        Assert.Equal(TimeSpan.FromSeconds(7), restored.RetryAfter);
        Assert.Null(restored.ResponseHeaders);
    }

    [Fact]
    public void OpenAiResponseFormatException_SerializesStatusCodeResponseContentsAndRequestUri()
    {
        var requestUri = new Uri("https://api.openai.com/v1/chat/completions");
        var exception = new OpenAiResponseFormatException(
            HttpStatusCode.OK,
            "{\"unexpected\":true}",
            "Unexpected response shape.",
            requestUri,
            new JsonException("bad payload"));

        var info = CaptureObjectData(exception);
        var restored = CreateFromSerializationInfo<OpenAiResponseFormatException>(info);

        Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
        Assert.Equal("{\"unexpected\":true}", restored.ResponseContents);
        Assert.Equal(requestUri, restored.RequestUri);
        Assert.IsType<JsonException>(restored.InnerException);
    }

    private static SerializationInfo CaptureObjectData(OpenAiException exception)
    {
        var info = new SerializationInfo(exception.GetType(), new FormatterConverter());
        var getObjectData = exception.GetType().GetMethod(
            "GetObjectData",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(getObjectData);
        getObjectData.Invoke(exception, new object[] { info, new StreamingContext(StreamingContextStates.All) });
        return info;
    }

    private static TException CreateFromSerializationInfo<TException>(SerializationInfo info)
        where TException : OpenAiException
    {
        var constructor = typeof(TException).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            types: new[] { typeof(SerializationInfo), typeof(StreamingContext) },
            modifiers: null);
        Assert.NotNull(constructor);
        return (TException)constructor.Invoke(
            new object[] { info, new StreamingContext(StreamingContextStates.All) });
    }
}
