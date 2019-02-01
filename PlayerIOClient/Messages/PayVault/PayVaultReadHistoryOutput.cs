using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultReadHistoryOutput
    {
        [ProtoMember(1)]
        public List<PayVaultHistoryEntry> Entries { get; set; }
    }
}