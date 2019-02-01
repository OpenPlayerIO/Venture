using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class LoadIndexRangeOutput 
    {
        [ProtoMember(1)]
        public List<BigDBObject> Objects { get; set; }
    }
}
