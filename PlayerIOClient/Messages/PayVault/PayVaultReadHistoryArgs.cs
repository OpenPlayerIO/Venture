using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultReadHistoryArgs
    {
        [ProtoMember(1)]
        public uint Page { get; set; }

        [ProtoMember(2)]
        public uint PageSize { get; set; }

        [ProtoMember(3)]
        public string ConnectUserId { get; set; }
    }
}