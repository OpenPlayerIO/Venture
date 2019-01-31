using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class CreateObjectsArgs
    {
        [ProtoMember(1)]
        public DatabaseObjectPushModel[] Objects { get; set; }

        [ProtoMember(2)]
        public bool LoadExisting { get; set; }
    }
}