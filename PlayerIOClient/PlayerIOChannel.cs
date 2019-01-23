using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using Flurl.Http;
using ProtoBuf;

namespace PlayerIOClient
{
    internal class PlayerIOChannel
    {
        public string Token { get; set; }

        public (bool success, TResponse response, PlayerIOError error) Request<TRequest, TResponse>(int method, TRequest args)
        {
            var channel = new FlurlClient("https://api.playerio.com/");

            var stream = new MemoryStream();
            Serializer.Serialize(stream, args);

            try
            {
                var response = channel.AllowHttpStatus(HttpStatusCode.OK)
                                       .WithHeader("playertoken", this.Token)
                                       .WithTimeout(seconds: 30)
                                       .Request($"api/{method}")
                                       .PostAsync(new ByteArrayContent(this.ReadAllBytes(stream))).ReceiveBytes().Result;

                if (this.ReadHeader(new MemoryStream(response)))
                {
                    return (true, Serializer.Deserialize<TResponse>(new MemoryStream(response.Skip(2).ToArray())), null);
                }
                else
                {
                    var error = Serializer.Deserialize<Error>(new MemoryStream(response.Skip(2).ToArray()));

                    return (false, default(TResponse), new PlayerIOError(error.ErrorCode, error.Message));
                }
            }
            catch
            {
                return (false, default(TResponse), 
                    new PlayerIOError(ErrorCode.GeneralError, "An error occurred while communicating with the Player.IO web service: the request timed out."));
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
