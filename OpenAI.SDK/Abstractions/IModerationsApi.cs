using OpenAI.SDK.Models.Moderations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAI.SDK.Abstractions
{
    public interface IModerationsApi
    {
        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<ModerationResponse> CreateModeration(string input, string model = null);

        Task<ModerationResponse> CreateModeration(string input, string model = null, CancellationToken cancellationToken = default);

        [Obsolete("Use the overload that accepts a CancellationToken.")]
        Task<ModerationResponse> CreateModeration(string[] input, string model = null);

        Task<ModerationResponse> CreateModeration(string[] input, string model = null, CancellationToken cancellationToken = default);
    }
}
