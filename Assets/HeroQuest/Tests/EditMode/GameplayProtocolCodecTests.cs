using HeroQuest.Net.Protocol;
using NUnit.Framework;
using UnityEngine;

namespace HeroQuest.Tests
{
    public sealed class GameplayProtocolCodecTests
    {
        [Test]
        public void EncodeHello_IncludesPlayerId()
        {
            var json = GameplayProtocolCodec.EncodeHello("tester");

            StringAssert.Contains("\"type\":\"hello\"", json);
            StringAssert.Contains("\"playerId\":\"tester\"", json);
        }

        [Test]
        public void EncodeMove_IncludesPositionAndInput()
        {
            var json = GameplayProtocolCodec.EncodeMove("tester", new Vector2(3.5f, -2f), new Vector2(1f, 0f));

            StringAssert.Contains("\"type\":\"move\"", json);
            StringAssert.Contains("\"positionX\":3.5", json);
            StringAssert.Contains("\"positionY\":-2.0", json);
            StringAssert.Contains("\"inputX\":1.0", json);
        }

        [Test]
        public void DecodeMonsterSpawn_ParsesServerPayload()
        {
            const string json = "{\"type\":\"spawn_monster\",\"monsterId\":\"m-001\",\"visualId\":\"archer\",\"positionX\":5.5,\"positionY\":-3.25}";

            var message = GameplayProtocolCodec.DecodeMonsterSpawn(json);

            Assert.AreEqual("m-001", message.monsterId);
            Assert.AreEqual("archer", message.visualId);
            Assert.AreEqual(5.5f, message.positionX);
            Assert.AreEqual(-3.25f, message.positionY);
        }
    }
}
