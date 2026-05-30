using System.Threading;
using System.Threading.Tasks;
using HeroQuest.Net.Auth;
using NUnit.Framework;

namespace HeroQuest.Tests
{
    public sealed class AuthServiceTests
    {
        [Test]
        public async Task LocalTestAuthService_AcceptsTestAccount()
        {
            var service = new LocalTestAuthService();

            var result = await service.LoginAsync("test", "test", CancellationToken.None);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("local-test-player", result.PlayerId);
        }

        [Test]
        public async Task LocalTestAuthService_RejectsWrongPassword()
        {
            var service = new LocalTestAuthService();

            var result = await service.LoginAsync("test", "wrong", CancellationToken.None);

            Assert.IsFalse(result.Success);
        }

        [Test]
        public void AuthProtocolCodec_EncodesLoginRequest()
        {
            var json = AuthProtocolCodec.EncodeLoginRequest("test", "test");

            StringAssert.Contains("\"type\":\"login\"", json);
            StringAssert.Contains("\"account\":\"test\"", json);
            StringAssert.Contains("\"password\":\"test\"", json);
        }
    }
}
