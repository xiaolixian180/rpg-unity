using System.Threading;
using System.Threading.Tasks;

namespace HeroQuest.Net.Auth
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string account, string password, CancellationToken cancellationToken);
    }
}
