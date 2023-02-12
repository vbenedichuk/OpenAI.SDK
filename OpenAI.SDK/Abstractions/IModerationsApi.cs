using OpenAI.SDK.Models.Moderations;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IModerationsApi
    {
        Task<ModerationResponse> CreateModeration(string input, string model = null);
        Task<ModerationResponse> CreateModeration(string[] input, string model = null);
    }
}
