using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultCreditArgs
    {
        [ProtoMember(1)]
        public uint Amount { get; set; }

        [ProtoMember(2)]
        public string Reason { get; set; }

        [ProtoMember(3)]
        public string ConnectUserId { get; set; }
    }
}