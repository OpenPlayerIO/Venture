using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Flurl.Http;
using ProtoBuf;

namespace PlayerIOClient
{
    internal class PlayerIOChannel
    {
        public string Token { get; set; }

        public (bool success, TResponse response, PlayerIOError error) Request<TRequest, TResponse>(int method, TRequest args)
        {
            var channel = new FlurlClient(PlayerIO.APIEndPoint);

            var stream = new MemoryStream();
            Serializer.Serialize(stream, args);

            try
            {
                var response = channel.AllowHttpStatus(HttpStatusCode.OK)
                                       .WithHeader("playertoken", this.Token)
                                       .WithTimeout(seconds: PlayerIO.APIRequestTimeout != -1 ? PlayerIO.APIRequestTimeout : 3600)
                                       .Request($"api/{method}")
                                       .PostAsync(new ByteArrayContent(this.ReadAllBytes(stream))).ReceiveBytes().Result;

                using (var responseStream = new MemoryStream(response))
                {
                    if (this.ReadHeader(responseStream))
                    {
                        return (true, Serializer.Deserialize<TResponse>(responseStream), null);
                    }
                    else
                    {
                        if (method != 403)
                        {
                            var error = Serializer.Deserialize<ErrorOutput>(responseStream);
                            return (false, default(TResponse), new PlayerIOError(error.ErrorCode, error.Message));
                        }
                        else
                        {
                            var error = Serializer.Deserialize<PlayerIORegistrationErrorOutput>(responseStream);
                            return (false, default(TResponse), new PlayerIORegistrationError((ErrorCode)error.ErrorCode, error.Message, error.UsernameError, error.PasswordError, error.EmailError, error.CaptchaError));
                        }
                    }
                }
            }
            catch (FlurlHttpException)
            {
                return (false, default(TResponse),
                    new PlayerIOError(ErrorCode.GeneralError, "An error occurred while communicating with the Player.IO web service: the request timed out."));
            }
            catch (Exception ex)
            {
                throw ex; // unknown exception is thrown.
            }
        }

        internal byte[] ReadAllBytes(Stream stream)
        {
            if (stream is MemoryStream memory)
                return memory.ToArray();

            using (var temp = new MemoryStream())
            {
                stream.CopyTo(temp);
                return temp.ToArray();
            }
        }

        private bool ReadHeader(Stream responseStream)
        {
            if (responseStream.ReadByte() == 1)
            {
                var length = (ushort)(responseStream.ReadByte() << 8 | responseStream.ReadByte());
                var array = new byte[length];

                responseStream.Read(array, 0, array.Length);
                this.Token = Encoding.UTF8.GetString(array, 0, array.Length);
            }

            return responseStream.ReadByte() == 1;
        }
    }
}
