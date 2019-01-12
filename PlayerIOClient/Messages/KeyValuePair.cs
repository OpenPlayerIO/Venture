using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class KeyValuePair
    {
        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2)]
        public string Value { get; set; }
    }
}
