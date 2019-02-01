using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultDebitOutput
    {
        [ProtoMember(1)]
        public PayVaultContents PayVaultContents { get; set; }
    }
}