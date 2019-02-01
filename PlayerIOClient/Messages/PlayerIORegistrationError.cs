using ProtoBuf;

namespace PlayerIOClient
{
    /// <summary>
    /// If there are any errors when using the SimpleRegister method of QuickConnect, you will get 
    /// back an error object of this type that holds more detail about the cause of the error. 
    /// You can use this information to provide better help for your users when they are 
    /// filling out your registration form.
    /// </summary>
    public class PlayerIORegistrationError : PlayerIOError
    {
        /// <summary> The error for the username field, if any. </summary>
        public string UsernameError { get; private set; }

        /// <summary> The error for the password field, if any .</summary>
        public string PasswordError { get; private set; }

        /// <summary> The error for the email field, if any. </summary>
        public string EmailError { get; private set; }

        /// <summary> The error for the captcha field, if any. </summary>
        public string CaptchaError { get; private set; }

        public PlayerIORegistrationError(ErrorCode errorCode, string message, string usernameError, string passwordError, string emailError, string captchaError) : base(errorCode, message)
        {
            this.UsernameError = usernameError;
            this.PasswordError = passwordError;
            this.EmailError = emailError;
            this.CaptchaError = captchaError;
        }
    }

    [ProtoContract]
    internal class PlayerIORegistrationErrorOutput
    {
        [ProtoMember(1)]
        public int ErrorCode { get; set; }

        [ProtoMember(2)]
        public string Message { get; set; }

        [ProtoMember(3)]
        public string UsernameError { get; set; }

        [ProtoMember(4)]
        public string PasswordError { get; set; }

        [ProtoMember(5)]
        public string EmailError { get; set; }

        [ProtoMember(6)]
        public string CaptchaError { get; set; }
    }
}
