using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class ServerEndpoint
    {
        [ProtoMember(1)]
        public string Address { get; set; }

        [ProtoMember(2)]
        public int Port { get; set; }
    }
}
