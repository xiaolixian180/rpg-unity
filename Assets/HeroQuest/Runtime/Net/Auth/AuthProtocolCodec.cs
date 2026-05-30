using System;
using UnityEngine;

namespace HeroQuest.Net.Auth
{
    [Serializable]
    public sealed class LoginRequestMessage
    {
        public string type = "login";
        public string account;
        public string password;
    }

    [Serializable]
    public sealed class LoginResponseMessage
    {
        public string type = "login_result";
        public bool success;
        public string playerId;
        public string message;
    }

    public static class AuthProtocolCodec
    {
        public static string EncodeLoginRequest(string account, string password)
        {
            return JsonUtility.ToJson(new LoginRequestMessage
            {
                account = account,
                password = password
            });
        }

        public static LoginResponseMessage DecodeLoginResponse(string json)
        {
            return JsonUtility.FromJson<LoginResponseMessage>(json);
        }
    }
}
