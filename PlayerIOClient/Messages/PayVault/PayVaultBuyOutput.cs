using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultBuyOutput
    {
        [ProtoMember(1)]
        public PayVaultContents PayVaultContents { get; set; }
    }
}