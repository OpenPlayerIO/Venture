using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultUsePaymentInfoArgs
    {
        [ProtoMember(1)]
        public string Provider { get; set; }

        [ProtoMember(2)]
        public List<KeyValuePair> ProviderArguments { get; set; }
    }
}