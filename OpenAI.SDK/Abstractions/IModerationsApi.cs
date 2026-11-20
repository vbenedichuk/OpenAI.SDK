using OpenAI.SDK.Models.Moderations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IModerationsApi
    {

        Task<ModerationResponse> CreateModerationAsync(string input, string model = null, CancellationToken cancellationToken = default);

        Task<ModerationResponse> CreateModerationAsync(string[] input, string model = null, CancellationToken cancellationToken = default);
    }
}
