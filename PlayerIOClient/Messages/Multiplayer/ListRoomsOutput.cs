using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class ListRoomsOutput
    {
        [ProtoMember(1)]
        public RoomInfo[] RoomInfo { get; set; }
    }
}
