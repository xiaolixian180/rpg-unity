namespace HeroQuest.Net.Go
{
    public readonly struct GoProtocolFrame
    {
        public GoProtocolFrame(ushort messageId, byte[] body)
        {
            MessageId = messageId;
            Body = body ?? System.Array.Empty<byte>();
        }

        public ushort MessageId { get; }
        public byte[] Body { get; }
    }
}
