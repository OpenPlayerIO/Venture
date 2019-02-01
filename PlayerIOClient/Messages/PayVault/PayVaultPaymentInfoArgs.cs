using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultPaymentInfoArgs
    {
        [ProtoMember(1)]
        public string Provider { get; set; }

        [ProtoMember(2)]
        public List<KeyValuePair> PurchaseArguments { get; set; }

        [ProtoMember(3)]
        public List<PayVaultBuyItemInfo> Items { get; set; }
    }


}