using ProtoBuf;

namespace PlayerIOClient
{
    /// <summary>
    /// The address and port where a server can be reached.
    /// </summary>
    [ProtoContract]
    public class ServerEndPoint
    {
        /// <summary>
        /// The address/hostname of the server.
        /// </summary>
        [ProtoMember(1)]
        public string Address { get; private set; }

        /// <summary>
        /// The port of the server.
        /// </summary>
        [ProtoMember(2)]
        public int Port { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"> The address/hostname of the server. </param>
        /// <param name="port"> The port of the server. </param>
        public ServerEndPoint(string address, int port)
        {
            this.Address = address;
            this.Port = port;
        }
    }
}
