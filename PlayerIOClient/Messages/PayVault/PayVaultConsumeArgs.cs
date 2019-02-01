using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultConsumeArgs
    {
        [ProtoMember(1)]
        public List<string> ItemIds { get; set; }

        [ProtoMember(2)]
        public string ConnectUserId { get; set; }
    }
}