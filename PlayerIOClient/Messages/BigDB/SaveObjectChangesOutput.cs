using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class SaveObjectChangesOutput
    {
        [ProtoMember(1)]
        public List<string> Versions { get; set; }
    }
}
