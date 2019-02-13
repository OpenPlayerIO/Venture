using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerIOClient
{
    public enum ProxyType
    {
        SOCKS4,
        SOCKS5,
        HTTPS
    }

    public class ProxyOptions
    {
        public ProxyOptions(ServerEndPoint endpoint, ProxyType type, string username, string password)
        {
            this.EndPoint = endpoint;
            this.Type = type;
            this.Username = username;
            this.Password = password;
        }

        public ServerEndPoint EndPoint { get; }
        public ProxyType Type { get; }
        public string Username { get; }
        public string Password { get; }
    }
}
