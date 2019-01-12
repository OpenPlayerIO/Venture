using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class SimpleUserGetSecureLoginInfoOutput
    {
        [ProtoMember(1)]
        public byte[] PublicKey { get; set; }

        [ProtoMember(2)]
        public string Nonce { get; set; }
    }
}
