using System.Threading;
using System.Threading.Tasks;

namespace Spotify.Api.Core.Interfaces
{
    public interface IAuthenticationClient
    {
        Task<string> AuthenticateAsync(CancellationToken cancellation = default);
    }
}
