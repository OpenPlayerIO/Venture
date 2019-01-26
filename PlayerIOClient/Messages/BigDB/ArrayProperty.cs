using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    public class ArrayProperty
    {
        [ProtoMember(1)]
        public int Index { get; set; }

        [ProtoMember(2)]
        public ValueObject Value { get; set; }
    }
}
