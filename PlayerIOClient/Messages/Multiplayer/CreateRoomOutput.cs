using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class CreateRoomOutput
    {
        [ProtoMember(1)]
        public string RoomId { get; set; }
    }
}
