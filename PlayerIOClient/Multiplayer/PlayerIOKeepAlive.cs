using System;
using System.Net.Sockets;

namespace PlayerIOClient
{
    internal class PlayerIOKeepAlive
    {
        public static void SetKeepAlive(Socket socket)
        {
            if (keepAliveValues == null)
            {
                var onOff = BitConverter.GetBytes(1u);
                var keepAliveTime = BitConverter.GetBytes(10000u);
                var keepAliveInterval = BitConverter.GetBytes(3000u);

                if (BitConverter.IsLittleEndian)
                {
                    keepAliveValues = new byte[]
                    {
                        onOff[0],
                        onOff[1],
                        onOff[2],
                        onOff[3],
                        keepAliveTime[0],
                        keepAliveTime[1],
                        keepAliveTime[2],
                        keepAliveTime[3],
                        keepAliveInterval[0],
                        keepAliveInterval[1],
                        keepAliveInterval[2],
                        keepAliveInterval[3]
                    };
                }
                else
                {
                    keepAliveValues = new byte[]
                    {
                        onOff[3],
                        onOff[2],
                        onOff[1],
                        onOff[0],
                        keepAliveTime[3],
                        keepAliveTime[2],
                        keepAliveTime[1],
                        keepAliveTime[0],
                        keepAliveInterval[3],
                        keepAliveInterval[2],
                        keepAliveInterval[1],
                        keepAliveInterval[0]
                    };
                }
            }
            try
            {
                socket.IOControl((IOControlCode)(-1744830460), keepAliveValues, null);
            }
            catch
            {
            }
        }

        private static byte[] keepAliveValues;
    }
}
