using ProtoBuf;

namespace PlayerIOClient
{

    [ProtoContract]
    internal class CreateRoomArgs
    {
        [ProtoMember(1)]
        public string RoomId { get; set; }

        [ProtoMember(2)]
        public string RoomType { get; set; }

        [ProtoMember(3)]
        public bool Visible { get; set; }

        [ProtoMember(4)]
        public KeyValuePair[] RoomData { get; set; }

        [ProtoMember(5)]
        public bool IsDevRoom { get; set; }
    }
}
