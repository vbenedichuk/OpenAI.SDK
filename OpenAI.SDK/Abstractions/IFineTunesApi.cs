using OpenAI.SDK.Models.FineTunes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Manage fine-tuning jobs to tailor a model to your specific training data.
    /// Related guide: <see href="https://platform.openai.com/docs/guides/fine-tuning">Fine-tune models</see>
    /// </summary>
    public interface IFineTunesApi
    {
        Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request);
        Task<List<FineTune>> ListFineTunesAsync();
        Task<FineTune> RetrieveFineTuneAsync(string fineTuneId);
        Task<FineTune> CancelFineTuneAsync(string fineTuneId);
        Task<List<FineTuneEvent>> ListFineTuneEvents(string fineTuneId, bool? stream = null);
        Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model);

    }
}
