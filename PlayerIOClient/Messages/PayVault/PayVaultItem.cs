using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultItem
    {
        [ProtoMember(1)]
        public string Id { get; set; }

        [ProtoMember(2)]
        public string ItemKey { get; set; }

        [ProtoMember(3)]
        public long PurchaseDate { get; set; }

        [ProtoMember(4)]
        public List<ObjectProperty> Properties { get; set; }
    }
}