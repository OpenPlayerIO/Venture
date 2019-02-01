using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultGiveArgs
    {
        [ProtoMember(1)]
        public List<PayVaultBuyItemInfo> Items { get; set; }

        [ProtoMember(2)]
        public string ConnectUserId { get; set; }
    }
}