using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultBuyArgs
    {
        [ProtoMember(1)]
        public List<PayVaultBuyItemInfo> Items { get; set; }

        [ProtoMember(2)]
        public bool KeepItems { get; set; }

        [ProtoMember(3)]
        public string ConnectUserId { get; set; }
    }
}