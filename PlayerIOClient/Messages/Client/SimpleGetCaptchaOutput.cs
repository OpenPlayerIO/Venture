using ProtoBuf;

namespace PlayerIOClient
{
    [ProtoContract]
    internal class SimpleGetCaptchaOutput
    {
        [ProtoMember(1)]
        internal string CaptchaKey { get; set; }

        [ProtoMember(2)]
        internal string CaptchaImageUrl { get; set; }
    }
}
