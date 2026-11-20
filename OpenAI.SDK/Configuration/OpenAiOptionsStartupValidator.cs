using System;
using Microsoft.Extensions.Options;

namespace OpenAI.SDK.Configuration
{
    /// <summary>
    /// Fails host startup when <see cref="OpenAiOptions"/> is invalid.
    /// </summary>
    internal sealed class OpenAiOptionsStartupValidator : IStartupValidator
    {
        private readonly IOptions<OpenAiOptions> _options;

        public OpenAiOptionsStartupValidator(IOptions<OpenAiOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public void Validate()
        {
            OpenAiOptionsValidator.Validate(_options.Value);
        }
    }
}
