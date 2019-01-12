namespace PlayerIOClient
{
    public class DevelopmentServer
    {
        public string Address { get; set; }
        public int Port { get; set; }

        public DevelopmentServer(string address, int port)
        {
            this.Address = address;
            this.Port = port;
        }
    }
}
