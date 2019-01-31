using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class ErrorOutput
    {
        [ProtoMember(1)]
        public ErrorCode ErrorCode { get; set; } = ErrorCode.InternalError;

        [ProtoMember(2)]
        public string Message { get; set; }
    }
}
