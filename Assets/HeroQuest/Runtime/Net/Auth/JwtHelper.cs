using System;
using System.Security.Cryptography;
using System.Text;

namespace HeroQuest.Net.Auth
{
    /// <summary>
    /// 客户端 JWT 生成器，与 Go 服务端共享 HMAC-SHA256 密钥。
    /// 生成的 token 格式: base64url(header).base64url(payload).base64url(signature)
    /// </summary>
    public static class JwtHelper
    {
        private const string Secret = "hero-quest-secret-key";
        private const int ExpireHours = 24;

        /// <summary>
        /// 为指定玩家生成 JWT 令牌。
        /// </summary>
        public static string GenerateToken(ulong playerId)
        {
            var header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";

            long iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long exp = iat + ExpireHours * 3600L;

            var payload = string.Format(
                "{{\"player_id\":{0},\"exp\":{1},\"iat\":{2}}}",
                playerId, exp, iat);

            string headerB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(header));
            string payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
            string signingInput = headerB64 + "." + payloadB64;

            byte[] keyBytes = Encoding.UTF8.GetBytes(Secret);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingInput));
                string signatureB64 = Base64UrlEncode(signature);
                return signingInput + "." + signatureB64;
            }
        }

        private static string Base64UrlEncode(byte[] data)
        {
            string b64 = Convert.ToBase64String(data);
            return b64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
