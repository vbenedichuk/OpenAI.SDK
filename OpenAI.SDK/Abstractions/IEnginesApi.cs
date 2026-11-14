using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Engines;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IEnginesApi
    {
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<List<EngineInfo>> ListEnginesAsync();

        Task<List<EngineInfo>> ListEnginesAsync(CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<EngineInfo> RetrieveEngine(string engineId);

        Task<EngineInfo> RetrieveEngine(string engineId, CancellationToken cancellationToken = default);
    }
}
