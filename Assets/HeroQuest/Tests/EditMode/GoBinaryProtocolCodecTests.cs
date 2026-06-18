using System.Text;
using HeroQuest.Net.Go;
using NUnit.Framework;

namespace HeroQuest.Tests
{
    public sealed class GoBinaryProtocolCodecTests
    {
        [Test]
        public void Encode_WritesLengthAndMessageIdInBigEndian()
        {
            var body = Encoding.UTF8.GetBytes("{\"token\":\"dev\"}");

            var frame = GoBinaryProtocolCodec.Encode(GoMessageIds.Login, body);

            Assert.AreEqual(0, frame[0]);
            Assert.AreEqual(2 + body.Length, frame[1]);
            Assert.AreEqual(3, frame[2]);
            Assert.AreEqual(233, frame[3]);
            Assert.AreEqual((byte)'{', frame[4]);
        }

        [Test]
        public void Decode_ReadsGoServerFrame()
        {
            var source = GoBinaryProtocolCodec.Encode(GoMessageIds.PlayerMove, Encoding.UTF8.GetBytes("{\"x\":1.5,\"y\":2.5}"));

            var frame = GoBinaryProtocolCodec.Decode(source);

            Assert.AreEqual(GoMessageIds.PlayerMove, frame.MessageId);
            Assert.AreEqual("{\"x\":1.5,\"y\":2.5}", GoBinaryProtocolCodec.GetBodyText(frame));
        }

        [Test]
        public void EncodeJson_UsesGoLoginRequestShape()
        {
            var source = GoBinaryProtocolCodec.EncodeJson(GoMessageIds.Login, new GoLoginRequest { token = "jwt-token" });
            var frame = GoBinaryProtocolCodec.Decode(source);

            Assert.AreEqual(GoMessageIds.Login, frame.MessageId);
            StringAssert.Contains("\"token\":\"jwt-token\"", GoBinaryProtocolCodec.GetBodyText(frame));
        }

        [Test]
        public void DecodeJson_ParsesEnterDungeonResponse()
        {
            const string json = "{\"code\":0,\"layer\":1,\"zone\":\"Forest\",\"monsters\":[{\"id\":101,\"name\":\"Slime\",\"hp\":10,\"max_hp\":10,\"x\":1.25,\"y\":-2.5}],\"players\":[],\"resources\":[]}";
            var frame = GoBinaryProtocolCodec.Decode(GoBinaryProtocolCodec.Encode(GoMessageIds.EnterDungeonResponse, Encoding.UTF8.GetBytes(json)));

            var response = GoBinaryProtocolCodec.DecodeJson<GoEnterDungeonResponse>(frame);

            Assert.AreEqual(0u, response.code);
            Assert.AreEqual(1, response.layer);
            Assert.AreEqual("Forest", response.zone);
            Assert.AreEqual(101ul, response.monsters[0].id);
            Assert.AreEqual(1.25d, response.monsters[0].x);
        }
    }
}
