using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class SaveObjectChangesArgs
    {
        [ProtoMember(1)]
        public LockType LockType { get; set; }

        [ProtoMember(2)]
        public List<BigDBChangeSet> ChangeSets { get; set; }

        [ProtoMember(3)]
        public bool CreateIfMissing { get; set; }
    }
}
