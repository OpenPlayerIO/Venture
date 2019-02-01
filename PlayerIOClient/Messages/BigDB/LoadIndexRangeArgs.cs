using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class LoadIndexRangeArgs
    {
        [ProtoMember(1)]
        public string Table { get; set; }

        [ProtoMember(2)]
        public string Index { get; set; }

        [ProtoMember(3)]
        public List<ValueObject> StartIndexValue { get; set; }

        [ProtoMember(4)]
        public List<ValueObject> StopIndexValue { get; set; }

        [ProtoMember(5)]
        public int Limit { get; set; }
    }
}
