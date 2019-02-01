using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultUsePaymentInfoOutput
    {
        [ProtoMember(1)]
        public List<KeyValuePair> ProviderResults { get; set; }

        [ProtoMember(2)]
        public PayVaultContents VaultContents { get; set; }
    }
}