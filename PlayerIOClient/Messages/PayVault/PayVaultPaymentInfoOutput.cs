using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultPaymentInfoOutput
    {
        [ProtoMember(1)]
        public List<KeyValuePair> ProviderArguments { get; set; }
    }


}