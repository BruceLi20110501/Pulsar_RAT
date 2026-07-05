using MessagePack;

namespace Pulsar.Common.Messages.Other
{
    [MessagePackObject]
    public sealed class SecureMessageEnvelope : IMessage
    {
        [Key(0)]
        public byte[] Payload { get; set; }
    }
}
