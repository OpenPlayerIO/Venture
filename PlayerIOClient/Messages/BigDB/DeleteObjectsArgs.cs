using System.Collections.Generic;
using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class DeleteObjectsArgs
    {
        [ProtoMember(1)]
        public List<BigDBObjectId> ObjectIds { get; set; }
    }
}