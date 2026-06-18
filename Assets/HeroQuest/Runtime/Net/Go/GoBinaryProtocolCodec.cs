using System;
using System.Text;
using UnityEngine;

namespace HeroQuest.Net.Go
{
    public static class GoBinaryProtocolCodec
    {
        public const int HeaderSize = 4;

        public static byte[] Encode(ushort messageId, byte[] body)
        {
            body ??= Array.Empty<byte>();
            var length = 2 + body.Length;
            if (length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(body), "Go protocol frame body is too large.");
            }

            var frame = new byte[HeaderSize + body.Length];
            WriteUInt16BigEndian(frame, 0, (ushort)length);
            WriteUInt16BigEndian(frame, 2, messageId);
            Buffer.BlockCopy(body, 0, frame, HeaderSize, body.Length);
            return frame;
        }

        public static byte[] EncodeJson<T>(ushort messageId, T payload)
        {
            var json = JsonUtility.ToJson(payload);
            return Encode(messageId, Encoding.UTF8.GetBytes(json));
        }

        public static GoProtocolFrame Decode(byte[] frame)
        {
            if (!TryDecode(frame, out var decoded))
            {
                throw new ArgumentException("Invalid Go protocol frame.", nameof(frame));
            }

            return decoded;
        }

        public static bool TryDecode(byte[] frame, out GoProtocolFrame decoded)
        {
            decoded = default;
            if (frame == null || frame.Length < HeaderSize)
            {
                return false;
            }

            var length = ReadUInt16BigEndian(frame, 0);
            if (length < 2)
            {
                return false;
            }

            var bodyLength = length - 2;
            if (frame.Length < HeaderSize + bodyLength)
            {
                return false;
            }

            var messageId = ReadUInt16BigEndian(frame, 2);
            var body = new byte[bodyLength];
            Buffer.BlockCopy(frame, HeaderSize, body, 0, bodyLength);
            decoded = new GoProtocolFrame(messageId, body);
            return true;
        }

        public static T DecodeJson<T>(GoProtocolFrame frame)
        {
            return JsonUtility.FromJson<T>(GetBodyText(frame));
        }

        public static string GetBodyText(GoProtocolFrame frame)
        {
            return Encoding.UTF8.GetString(frame.Body);
        }

        private static ushort ReadUInt16BigEndian(byte[] data, int offset)
        {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        private static void WriteUInt16BigEndian(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value >> 8);
            data[offset + 1] = (byte)(value & 0xFF);
        }
    }
}
