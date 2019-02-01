using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultConsumeOutput
    {
        [ProtoMember(1)]
        public PayVaultContents PayVaultContents { get; set; }
    }
}