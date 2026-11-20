using OpenAI.SDK.Models.Engines;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    [Obsolete("API is not supported anymore")]
    public interface IEnginesApi
    {

        Task<List<EngineInfo>> ListEnginesAsync(CancellationToken cancellationToken = default);

        Task<EngineInfo> RetrieveEngineAsync(string engineId, CancellationToken cancellationToken = default);
    }
}
