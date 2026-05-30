using System;
using HeroQuest.Domain;

namespace HeroQuest.Net.Protocol
{
    [Serializable]
    public sealed class ClientRequestEnvelope
    {
        public string type;
        public string requestId;
        public string token;
        public string payloadJson;
    }

    [Serializable]
    public sealed class ServerResponseEnvelope
    {
        public string type;
        public string requestId;
        public GameErrorCode errorCode;
        public string message;
        public string payloadJson;
    }
}
