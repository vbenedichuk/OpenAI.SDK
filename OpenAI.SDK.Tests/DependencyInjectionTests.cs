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
        Assert.NotNull(provider.GetService<ICompletionsApi>());
        Assert.NotNull(provider.GetService<IEditsApi>());
        Assert.NotNull(provider.GetService<IImagesApi>());
        Assert.NotNull(provider.GetService<IEmbeddingsApi>());
        Assert.NotNull(provider.GetService<IFilesApi>());
        Assert.NotNull(provider.GetService<IFineTunesApi>());
        Assert.NotNull(provider.GetService<IChatApi>());
        Assert.NotNull(provider.GetService<IModerationsApi>());
        Assert.NotNull(provider.GetService<IEnginesApi>());
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
}
