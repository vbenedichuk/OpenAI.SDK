using System;
using OpenAI.SDK.Exceptions;

namespace OpenAI.SDK.SampleConsoleApp
{
    /// <summary>
    /// Formats SDK exceptions for console output.
    /// </summary>
    internal static class ExceptionReport
    {
        /// <summary>
        /// Returns the exception message and, when available, the captured response body.
        /// </summary>
        public static string ToText(OpenAiException exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var responseContents = (exception as OpenAiApiException)?.ResponseContents
                ?? (exception as OpenAiResponseFormatException)?.ResponseContents;

            return string.IsNullOrWhiteSpace(responseContents)
                ? exception.Message
                : exception.Message + Environment.NewLine + responseContents;
        }
    }
}
