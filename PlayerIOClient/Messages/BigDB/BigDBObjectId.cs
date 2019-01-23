using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class BigDBObjectId
    {
        [ProtoMember(1)]
        public string Table { get; set; }

        [ProtoMember(2)]
        public List<string> Keys { get; set; }
    }
}
