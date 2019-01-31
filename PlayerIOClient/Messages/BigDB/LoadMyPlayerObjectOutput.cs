using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class LoadMyPlayerObjectOutput
    {
        [ProtoMember(1)]
        public BigDBObject PlayerObject { get; set; }
    }
}
