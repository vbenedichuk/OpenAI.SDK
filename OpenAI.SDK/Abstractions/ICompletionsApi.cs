using OpenAI.SDK.Models.Completions;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Given a prompt, the model will return one or more predicted completions, and can also return the probabilities of alternative tokens at each position.
    /// </summary>
    public interface ICompletionsApi
    {

        /// <summary>
        /// POST https://api.openai.com/v1/completions 
        /// Creates a completion for the provided prompt and parameters
        /// </summary>
        /// <param name="apiCompletionRequest"></param>
        /// <returns></returns>
        Task<ApiCompletionResponse> CreateCompletionAsync(ApiCompletionRequest apiCompletionRequest);

    }
}
