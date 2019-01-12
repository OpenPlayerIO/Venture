using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PlayerInsightState
    {
        [ProtoMember(1)]
        public int PlayersOnline { get; set; }

        [ProtoMember(2)]
        public KeyValuePair[] Segments { get; set; }
    }
}
