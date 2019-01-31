using System.Collections.Generic;
using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class CreateObjectsOutput
    {
        [ProtoMember(1)]
        public List<BigDBObject> Objects { get; set; }
    }
}