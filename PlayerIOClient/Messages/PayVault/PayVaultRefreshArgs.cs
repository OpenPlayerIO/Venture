﻿using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultRefreshArgs
    {
        [ProtoMember(1)]
        public string LastVersion { get; set; }

        [ProtoMember(2)]
        public string ConnectUserId { get; set; }
    }
}