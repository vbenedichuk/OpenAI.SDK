using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.SDK.Configuration;

namespace OpenAI.SDK.Tests;

public class ConfigurationValidationTests
{
    [Fact]
    public void StartupValidation_ThrowsForMissingApiKey()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.BaseUrl = "https://api.openai.com/v1/";
        });
        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IStartupValidator>();

        var exception = Assert.Throws<ArgumentException>(() => validator.Validate());
        Assert.Contains("ApiKey", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StartupValidation_ThrowsForBaseUrlWithQuery()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.openai.com/v1/?x=1";
        });
        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IStartupValidator>();

        var exception = Assert.Throws<ArgumentException>(() => validator.Validate());
        Assert.Contains("BaseUrl", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StartupValidation_PassesForValidOptions()
    {
        var services = new ServiceCollection();
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "test-key";
            options.BaseUrl = "https://api.openai.com/v1/";
        });
        services.AddOpenAiApi();

        using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IStartupValidator>();

        validator.Validate();
    }
}
