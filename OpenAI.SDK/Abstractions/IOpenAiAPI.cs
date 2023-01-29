using OpenAI.SDK.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IOpenAiAPI
    {
        Task<List<ApiModel>> GetModels();
    }
}
