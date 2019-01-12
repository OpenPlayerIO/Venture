using System.Net.Sockets;
using System.Threading;

namespace PlayerIOClient
{
    public static class PortCheck
    {
        public static bool IsPortOpen(string host, int port, int timeout, int retry)
        {
            var retryCount = 0;
            while (retryCount < retry)
            {
                if (retryCount > 0)
                    Thread.Sleep(timeout);

                try
                {
                    using (var client = new TcpClient())
                    {
                        var result = client.BeginConnect(host, port, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(timeout);
                        if (success)
                            return true;

                        client.EndConnect(result);
                    }
                }
                catch
                {
                }
                finally { retryCount++; }
            }

            return false;
        }
    }
}
