using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class AuthenticateStartDialog
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public KeyValuePair[] Arguments { get; set; }
    }
}
