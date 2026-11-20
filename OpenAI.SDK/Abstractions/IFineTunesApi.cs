using OpenAI.SDK.Models.FineTunes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    /// <summary>
    /// Manage fine-tuning jobs to tailor a model to your specific training data.
    /// Related guide: <see href="https://platform.openai.com/docs/guides/fine-tuning">Fine-tune models</see>
    /// </summary>
    [Obsolete("API is not supported anymore")]
    public interface IFineTunesApi
    {

        Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request, CancellationToken cancellationToken = default);

        Task<List<FineTune>> ListFineTunesAsync(CancellationToken cancellationToken = default);

        Task<FineTune> RetrieveFineTuneAsync(string fineTuneId, CancellationToken cancellationToken = default);

        Task<FineTune> CancelFineTuneAsync(string fineTuneId, CancellationToken cancellationToken = default);

        Task<List<FineTuneEvent>> ListFineTuneEventsAsync(string fineTuneId, bool? stream = null, CancellationToken cancellationToken = default);

        Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model, CancellationToken cancellationToken = default);

    }
}
