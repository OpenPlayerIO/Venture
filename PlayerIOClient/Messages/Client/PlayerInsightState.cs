using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class PlayerInsightState
    {
        [ProtoMember(1)]
        public int PlayersOnline { get; set; }

        [ProtoMember(2)]
        public List<KeyValuePair> Segments { get; set; }
    }
}