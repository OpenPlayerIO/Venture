using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class CreateJoinRoomArgs
    {
        [ProtoMember(1)]
        public string RoomId { get; set; }

        [ProtoMember(2)]
        public string RoomType { get; set; }

        [ProtoMember(3)]
        public bool Visible { get; set; }

        [ProtoMember(4)]
        public Dictionary<string, string> RoomData { get; set; }

        [ProtoMember(5)]
        public Dictionary<string, string> JoinData { get; set; }

        [ProtoMember(6)]
        public bool IsDevRoom { get; set; }
    }
}
