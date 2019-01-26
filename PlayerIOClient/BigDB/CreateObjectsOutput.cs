using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class CreateObjectsOutput
    {
        [ProtoMember(1)]
        public DatabaseObject[] Objects { get; set; }
    }
}