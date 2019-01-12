using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PlayerIOClient
{
    internal static class PlayerIOAuth
    {
        internal static (byte[] publicKey, string nonce) SimpleUserGetSecureLoginInfo()
        {
            var (success, response, error) = new PlayerIOChannel().Request<Empty, SimpleUserGetSecureLoginInfoOutput>(424, new Empty());

            if (!success)
            {
                throw new PlayerIOError(error.ErrorCode, error.Message);
            }

            return (response.PublicKey, response.Nonce);
        }

        internal static string SimpleUserPasswordEncrypt(byte[] certificateBytes, string password)
        {
            var rsacryptoServiceProvider = new RSACryptoServiceProvider(new CspParameters
            {
                ProviderType = 1
            });

            rsacryptoServiceProvider.ImportCspBlob(certificateBytes);
            var bytes = Encoding.UTF8.GetBytes(password);
            var inArray = rsacryptoServiceProvider.Encrypt(bytes, false);
            return Convert.ToBase64String(inArray);
        }

        internal static Dictionary<string, string> GetClientInfo()
        {
            var dictionary = new Dictionary<string, string>();

            try
            {
                dictionary["os"] = Environment.OSVersion.Platform + "/" + Environment.OSVersion.VersionString;
            }
            catch
            {

            }

            return dictionary;
        }
    }
}
