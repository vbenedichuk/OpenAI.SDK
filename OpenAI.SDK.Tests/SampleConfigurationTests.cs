using Microsoft.Extensions.Configuration;
using OpenAI.SDK.SampleConsoleApp;

namespace OpenAI.SDK.Tests;

public class SampleConfigurationTests
{
    [Fact]
    public void Load_UsesConfiguredApiKeyOverEnvironmentVariable()
    {
        var configuration = BuildConfiguration(("OpenAI:Api:ApiKey", "configured-key"));

        var options = SampleConfiguration.Load(configuration, _ => "environment-key");

        Assert.Equal("configured-key", options.ApiKey);
    }

    [Fact]
    public void Load_FallsBackToEnvironmentVariable_WhenConfiguredApiKeyIsBlank()
    {
        var configuration = BuildConfiguration(("OpenAI:Api:ApiKey", "   "));

        var options = SampleConfiguration.Load(configuration, name =>
        {
            Assert.Equal("OPENAI_API_KEY", name);
            return "environment-key";
        });

        Assert.Equal("environment-key", options.ApiKey);
    }

    [Fact]
    public void Load_BindsBaseUrlAndTimeout()
    {
        var configuration = BuildConfiguration(
            ("OpenAI:Api:ApiKey", "configured-key"),
            ("OpenAI:Api:BaseUrl", "https://example.test/v1/"),
            ("OpenAI:Api:Timeout", "00:00:30"));

        var options = SampleConfiguration.Load(configuration, _ => null);

        Assert.Equal("https://example.test/v1/", options.BaseUrl);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
    }

    [Fact]
    public void Load_ReturnsEmptyApiKey_WhenNothingIsConfigured()
    {
        var configuration = BuildConfiguration();

        var options = SampleConfiguration.Load(configuration, _ => null);

        Assert.True(string.IsNullOrWhiteSpace(options.ApiKey));
    }

    private static IConfiguration BuildConfiguration(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(pair => pair.Key, pair => (string?)pair.Value))
            .Build();
    }
}
