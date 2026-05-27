using System;
using UnityEngine;

namespace HeroQuest.Net.Protocol
{
    [Serializable]
    public sealed class ClientHelloMessage
    {
        public string type = "hello";
        public string playerId;
        public string clientVersion = "prototype";
    }

    [Serializable]
    public sealed class ClientMoveMessage
    {
        public string type = "move";
        public string playerId;
        public float positionX;
        public float positionY;
        public float inputX;
        public float inputY;
    }

    [Serializable]
    public sealed class MonsterSpawnMessage
    {
        public string type = "spawn_monster";
        public string monsterId;
        public string visualId;
        public float positionX;
        public float positionY;
    }

    public static class GameplayProtocolCodec
    {
        public static string EncodeHello(string playerId)
        {
            return JsonUtility.ToJson(new ClientHelloMessage
            {
                playerId = playerId
            });
        }

        public static string EncodeMove(string playerId, Vector2 position, Vector2 input)
        {
            return JsonUtility.ToJson(new ClientMoveMessage
            {
                playerId = playerId,
                positionX = position.x,
                positionY = position.y,
                inputX = input.x,
                inputY = input.y
            });
        }

        public static MonsterSpawnMessage DecodeMonsterSpawn(string json)
        {
            return JsonUtility.FromJson<MonsterSpawnMessage>(json);
        }
    }
}
