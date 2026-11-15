using Microsoft.Extensions.DependencyInjection;
using OpenAI.SDK.Abstractions;
using OpenAI.SDK.Configuration;

namespace OpenAI.SDK.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddOpenAiApi_RegistersHttpClientFactory()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.openai.com/v1/";
        });

        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IHttpClientFactory>());
    }

    [Fact]
    public void AddOpenAiApi_RegistersEverySupportedApi()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.openai.com/v1/";
        });

        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IModelsApi>());
#pragma warning disable CS0612 // Retired APIs stay registered for backward compatibility.
        Assert.NotNull(provider.GetService<ICompletionsApi>());
        Assert.NotNull(provider.GetService<IEditsApi>());
#pragma warning restore CS0612
        Assert.NotNull(provider.GetService<IImagesApi>());
        Assert.NotNull(provider.GetService<IEmbeddingsApi>());
        Assert.NotNull(provider.GetService<IFilesApi>());
#pragma warning disable CS0612 // Retired APIs stay registered for backward compatibility.
        Assert.NotNull(provider.GetService<IFineTunesApi>());
#pragma warning restore CS0612
        Assert.NotNull(provider.GetService<IChatApi>());
        Assert.NotNull(provider.GetService<IModerationsApi>());
#pragma warning disable CS0612 // Retired APIs stay registered for backward compatibility.
        Assert.NotNull(provider.GetService<IEnginesApi>());
#pragma warning restore CS0612
    }

    [Fact]
    public void AddOpenAiApi_CanResolveApiWithoutCallerRegisteringHttpClient()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.openai.com/v1/";
        });

        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<IChatApi>());
    }

    [Fact]
    public void ResolvingApi_ThrowsClearException_WhenApiKeyMissing()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.BaseUrl = "https://api.openai.com/v1/";
        });

        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        var exception = Assert.Throws<ArgumentException>(() => provider.GetRequiredService<IChatApi>());
        Assert.IsNotType<ArgumentNullException>(exception);
        Assert.Contains("ApiKey", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolvingApi_ThrowsClearException_WhenBaseUrlInvalid()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "not-a-url";
        });

        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        var exception = Assert.Throws<ArgumentException>(() => provider.GetRequiredService<IChatApi>());
        Assert.IsNotType<ArgumentNullException>(exception);
        Assert.Contains("BaseUrl", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolvingApi_ThrowsClearException_WhenBaseUrlIsNotHttp()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "ftp://api.openai.com/v1/";
        });

        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        var exception = Assert.Throws<ArgumentException>(() => provider.GetRequiredService<IChatApi>());
        Assert.Contains("BaseUrl", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
