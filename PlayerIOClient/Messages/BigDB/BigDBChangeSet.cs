using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class BigDBChangeSet
    {
        [ProtoMember(1)]
        public string Table { get; set; }

        [ProtoMember(2)]
        public string Key { get; set; }

        [ProtoMember(3)]
        public string OnlyIfVersion { get; set; }

        [ProtoMember(4)]
        public List<ObjectProperty> Changes { get; set; }

        [ProtoMember(5)]
        public bool FullOverwrite { get; set; }
    }
}
