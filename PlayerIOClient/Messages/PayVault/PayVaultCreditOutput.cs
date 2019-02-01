using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultCreditOutput
    {
        [ProtoMember(1)]
        public PayVaultContents PayVaultContents { get; set; }
    }
}