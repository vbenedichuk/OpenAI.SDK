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
    [Obsolete]
    public interface IFineTunesApi
    {
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request);

        Task<FineTune> CreateFineTuneAsync(CreateFineTuneRequest request, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<List<FineTune>> ListFineTunesAsync();

        Task<List<FineTune>> ListFineTunesAsync(CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<FineTune> RetrieveFineTuneAsync(string fineTuneId);

        Task<FineTune> RetrieveFineTuneAsync(string fineTuneId, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<FineTune> CancelFineTuneAsync(string fineTuneId);

        Task<FineTune> CancelFineTuneAsync(string fineTuneId, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<List<FineTuneEvent>> ListFineTuneEvents(string fineTuneId, bool? stream = null);

        Task<List<FineTuneEvent>> ListFineTuneEventsAsync(string fineTuneId, bool? stream = null, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model);

        Task<DeleteFineTuneResponse> DeleteFineTuneModelAsync(string model, CancellationToken cancellationToken = default);

    }
}
