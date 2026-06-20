using System.Threading;
using HeroQuest.Net.Auth;
using NUnit.Framework;

namespace HeroQuest.Tests
{
    public sealed class AuthServiceTests
    {
        /// <summary>
        /// 在 EditMode 下没有真实服务器，但 LocalTestAuthService 在账号/密码不匹配时
        /// 应在触碰网络之前直接返回失败结果。
        /// </summary>
        [Test]
        public void LocalTestAuthService_RejectsWrongPassword()
        {
            var service = new LocalTestAuthService(null);

            var result = service.LoginAsync("test", "wrong", CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsFalse(result.Success);
            Assert.AreEqual("账号或密码错误", result.Message);
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
