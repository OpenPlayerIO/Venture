﻿using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PayVaultDebitArgs
    {
        [ProtoMember(1)]
        public uint Amount { get; set; }

        [ProtoMember(2)]
        public string Reason { get; set; }

        [ProtoMember(3)]
        public string TargetUserId { get; set; }
    }
}