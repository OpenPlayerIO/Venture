﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PlayerIOClient
{
    internal static class PlayerIOAuth
    {
        internal static (byte[] publicKey, string nonce) SimpleUserGetSecureLoginInfo()
        {
            var (success, response, error) = new PlayerIOChannel().Request<EmptyOutput, SimpleUserGetSecureLoginInfoOutput>(424, new EmptyOutput());

            if (!success)
                throw new PlayerIOError(error.ErrorCode, error.Message);

            return (response.PublicKey, response.Nonce);
        }

        internal static string SimpleUserPasswordEncrypt(byte[] certificateBytes, string password)
        {
            var provider = new RSACryptoServiceProvider(new CspParameters { ProviderType = 1 });

            provider.ImportCspBlob(certificateBytes);

            return Convert.ToBase64String(provider.Encrypt(Encoding.UTF8.GetBytes(password), false));
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
