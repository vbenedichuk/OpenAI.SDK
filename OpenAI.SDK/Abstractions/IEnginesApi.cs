using OpenAI.SDK.Models;
using OpenAI.SDK.Models.Engines;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IEnginesApi
    {
        Task<List<EngineInfo>> ListEnginesAsync();
        Task<EngineInfo> RetrieveEngine(string engineId);
    }
}
