using ProtoBuf;
using System.Collections.Generic;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class SimpleRegisterArgs
    {
        [ProtoMember(1)]
        public string GameId { get; set; }

        [ProtoMember(2)]
        public string Username { get; set; }

        [ProtoMember(3)]
        public string Password { get; set; }

        [ProtoMember(4)]
        public string Email { get; set; }

        [ProtoMember(5)]
        public List<KeyValuePair> ExtraData { get; set; }

        [ProtoMember(6)]
        public string CaptchaKey { get; set; }

        [ProtoMember(7)]
        public string CaptchaValue { get; set; }

        [ProtoMember(8)]
        public string PartnerId { get; set; }

        [ProtoMember(9)]
        public string[] PlayerInsightSegments { get; set; }

        [ProtoMember(10)]
        public string ClientAPI { get; set; }

        [ProtoMember(11)]
        public List<KeyValuePair> ClientInfo { get; set; }
    }
}
