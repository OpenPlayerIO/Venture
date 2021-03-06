﻿using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class ObjectProperty
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public ValueObject Value { get; set; }
    }
}
