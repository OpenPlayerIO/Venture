using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class BigDBObject
    {
        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2)]
        public string Version { get; set; }

        [ProtoMember(3)]
        public List<ObjectProperty> Properties { get; set; }

        [ProtoMember(4)]
        public uint Creator { get; set; }
    }
}
