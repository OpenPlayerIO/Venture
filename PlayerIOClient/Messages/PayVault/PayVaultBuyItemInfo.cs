using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultBuyItemInfo
    {
        [ProtoMember(1)]
        public string ItemKey { get; set; }

        [ProtoMember(2)]
        public List<ObjectProperty> Payload { get; set; }
    }
}