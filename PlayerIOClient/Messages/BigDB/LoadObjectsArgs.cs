using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class LoadObjectsArgs
    {
        [ProtoMember(1)]
        public BigDBObjectId ObjectIds { get; set; }
    }
}
