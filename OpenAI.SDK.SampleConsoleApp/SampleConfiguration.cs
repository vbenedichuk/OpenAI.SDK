using Microsoft.Extensions.Configuration;
using OpenAI.SDK.Configuration;
using System;

namespace OpenAI.SDK.SampleConsoleApp
{
    /// <summary>
    /// Loads the sample's SDK options from configuration and the environment.
    /// </summary>
    internal static class SampleConfiguration
    {
        /// <summary>
        /// Binds the <c>OpenAI:Api</c> section and falls back to
        /// <c>OPENAI_API_KEY</c> when no API key is configured.
        /// </summary>
        public static OpenAiOptions Load(IConfiguration configuration, Func<string, string?> getEnvironmentVariable)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (getEnvironmentVariable == null)
            {
                throw new ArgumentNullException(nameof(getEnvironmentVariable));
            }

            var options = new OpenAiOptions();
            configuration.GetSection("OpenAI:Api").Bind(options);

            if (string.IsNullOrWhiteSpace(options.ApiKey))
            {
                var environmentApiKey = getEnvironmentVariable("OPENAI_API_KEY");
                if (!string.IsNullOrWhiteSpace(environmentApiKey))
                {
                    options.ApiKey = environmentApiKey;
                }
            }

            return options;
        }
    }
}
