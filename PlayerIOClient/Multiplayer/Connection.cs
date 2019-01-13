using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static PlayerIOClient.BinaryDeserializer;

namespace PlayerIOClient
{
    /// <summary> Used to add a message handler to the OnMessage event of an instance of Connection. </summary>
    public delegate void MessageReceivedEventHandler(object sender, Message e);

    /// <summary>
    /// Used to add a disconnect handler to the OnDisconnect event of an instance of Connection.
    /// </summary>
    /// <param name="message"> The reason of disconnecting explained by words. </param>
    public delegate void DisconnectEventHandler(object sender, string message);

    /// <summary> A connection to a running Player.IO multiplayer room. </summary>
    public class Connection
    {
        /// <summary> Used to add a message handler to the OnMessage event of an instance of Connection. </summary>
        public event MessageReceivedEventHandler OnMessage;

        /// <summary>
        /// Used to add a disconnect handler to the OnDisconnect event of an instance of Connection.
        /// </summary>
        /// <param name="message"> The reason of disconnecting explained by words. </param>
        public event DisconnectEventHandler OnDisconnect;

        internal Connection(IPEndPoint endpoint, string joinKey)
        {
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.Connect(endpoint.Address, endpoint.Port);

            this.Stream = new NetworkStream(this.Socket);
            this.Serializer = new BinarySerializer();
            this.Deserializer = new BinaryDeserializer();

            this.Stream.BeginRead(this.Buffer, 0, this.Buffer.Length, new AsyncCallback(this.ReceiveCallback), null);
            this.Socket.Send(new byte[] { 0 });
            this.Send(new Message("join", joinKey));

            this.Deserializer.OnDeserializedMessage += (message) =>
            {
                OnMessage?.Invoke(this, message);
            };
        }

        private Socket Socket { get; set; }
        private Stream Stream { get; set; }
        private byte[] Buffer { get; set; } = new byte[ushort.MaxValue];

        private BinarySerializer Serializer;
        private BinaryDeserializer Deserializer;

        public void Send(string type, params object[] arguments) => this.Send(new Message(type, arguments));

        public void Send(Message message)
        {
            if (this.Socket != null && this.Socket.Connected)
                this.Socket.Send(Serializer.Serialize(message));
        }

        public void Disconnect()
        {
            if (this.Socket != null && this.Socket.Connected)
            {
                this.Socket.Disconnect(false);
                this.OnDisconnect?.Invoke(this, "The connection was forcibly terminated by local client.");

                this.Socket.Dispose();
                this.Buffer = null;
                this.Stream = null;
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (!this.Socket.Connected)
            {
                if (this.Stream != null)
                    this.OnDisconnect?.Invoke(this, "The connection was forcibly reset by peer.");

                return;
            }

            var length = this.Stream.EndRead(ar);
            var received = this.Buffer.Take(length).ToArray();

            if (length == 0)
            {
                this.OnDisconnect?.Invoke(this, "The connection was forcibly reset by peer. (receivedBytes == 0)");
                return;
            }

            this.Deserializer.AddBytes(received);
            this.Stream.BeginRead(this.Buffer, 0, this.Buffer.Length, new AsyncCallback(this.ReceiveCallback), null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}