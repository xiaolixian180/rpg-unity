using System.Threading;
using System.Threading.Tasks;

namespace HeroQuest.Net.Auth
{
    public sealed class LocalTestAuthService : IAuthService
    {
        public const string TestAccount = "test";
        public const string TestPassword = "test";

        public Task<AuthResult> LoginAsync(string account, string password, CancellationToken cancellationToken)
        {
            var isValid = account == TestAccount && password == TestPassword;
            var result = isValid
                ? new AuthResult(true, "local-test-player", "Login success")
                : new AuthResult(false, string.Empty, "Account or password is invalid");

            return Task.FromResult(result);
        }
    }
}
