namespace PlayerIOClient
{
    public class SimpleCaptcha
    {
        /// <summary>
        /// The key for this captcha image. This value must be kept and sent to the simple registration method along with the string from the user.
        /// </summary>
        public string CaptchaKey { get; private set; }

        /// <summary>
        /// A URL for the captcha image. You must show the image to the user, and ask what text is shown in the image.
        /// </summary>
        public string CaptchaImageUrl { get; private set; }

        internal SimpleCaptcha(string captchaKey, string captchaImageUrl)
        {
            this.CaptchaKey = captchaKey;
            this.CaptchaImageUrl = captchaImageUrl;
        }
    }
}
