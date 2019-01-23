using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class LoadObjectsOutput
    {
        [ProtoMember(1)]
        public List<BigDBObject> Objects { get; set; }
    }
}
