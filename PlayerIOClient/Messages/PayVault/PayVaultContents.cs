using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultContents
    {
        [ProtoMember(1)]
        public string Version { get; set; }

        [ProtoMember(2)]
        public int Coins { get; set; }

        [ProtoMember(3)]
        public List<PayVaultItem> Items { get; set; }
    }
}