using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultGiveOutput
    {
        [ProtoMember(1)]
        public PayVaultContents PayVaultContents { get; set; }
    }
}