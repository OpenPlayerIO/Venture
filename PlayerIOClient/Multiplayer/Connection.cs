using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Org.Mentalis.Network.ProxySocket;
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
        /// <summary>
        /// A property used to add a message handler to the OnMessage event of an instance of Connection.
        /// </summary>
        public event MessageReceivedEventHandler OnMessage;

        /// <summary>
        /// A property used to add a disconnect handler to the OnDisconnect event of an instance of Connection.
        /// </summary>
        /// <param name="message"> The reason for the disconnection. </param>
        public event DisconnectEventHandler OnDisconnect;

        /// <summary>
        /// A boolean representing whether the connection is established with a remote host.
        /// </summary>
        public bool Connected => this.Socket.Connected;

        /// <summary>
        /// The remote endpoint of the current connection.
        /// </summary>
        public IPEndPoint RemoteEndPoint => this.Socket.RemoteEndPoint as IPEndPoint;

        /// <summary>
        /// The bandwidth information pertaining to the current connection.
        /// </summary>
        /// <returns> A brief summary of the bandwith used by the connection. </returns>
        public (ulong messages_sent, ulong messages_received, ulong bytes_sent, ulong bytes_received) GetBandwith() =>
            (TotalMessagesSent, TotalMessagesReceived, TotalBytesSent, TotalBytesReceived);

        /// <summary>
        /// An optional means of instantiating a connection from a join key, for advanced usage purposes.
        /// A typical game should most likely use the CreateJoinRoom/JoinRoom methods in a client instance instead.
        /// </summary>
        /// <param name="endpoint"> The game server endpoint. </param>
        /// <param name="joinKey"> The key provided by the API for the respective game server. </param>
        /// <param name="joinData"> An optional dictionary containing join data to send during join. </param>
        /// <param name="proxy"> An optional proxy which, if specified, will be used during the connection. </param>
        public static Connection Create(ServerEndPoint endpoint, string joinKey, Dictionary<string, string> joinData = null, ProxyOptions proxy = null)
            => new Connection(new IPEndPoint(Dns.GetHostAddresses(endpoint.Address).First(), endpoint.Port), joinKey, joinData, proxy);

        internal Connection(IPEndPoint endpoint, string joinKey, Dictionary<string, string> joinData = null, ProxyOptions proxy = null)
        {
            this.Socket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (proxy != null)
            {
                this.Socket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ProxyType = proxy.Type == ProxyType.SOCKS4 ? ProxyTypes.Socks4 : proxy.Type == ProxyType.SOCKS5 ? ProxyTypes.Socks5 : ProxyTypes.None,
                    ProxyEndPoint = new IPEndPoint(Dns.GetHostAddresses(proxy.EndPoint.Address).First(), proxy.EndPoint.Port),
                };

                if (!string.IsNullOrEmpty(proxy.Username))
                    this.Socket.ProxyUser = proxy.Username;

                if (!string.IsNullOrEmpty(proxy.Password))
                    this.Socket.ProxyUser = proxy.Password;
            }

            this.Socket.Connect(endpoint.Address.ToString(), endpoint.Port);

            this.Stream = new NetworkStream(this.Socket);
            this.MessageDeserializer = new BinaryDeserializer();

            this.Stream.BeginRead(this.Buffer, 0, this.Buffer.Length, new AsyncCallback(this.ReceiveCallback), null);
            this.Socket.Send(new byte[] { 0 });

            PlayerIOKeepAlive.SetKeepAlive(this.Socket); // Player.IO uses these keep alive values in their client.

            this.Socket.NoDelay = true; // Player.IO enables the Nagle algorithm in their client.
            this.Socket.Blocking = true; // Player.IO enables the blocking mode in their client.

            var join = new Message("join", joinKey);

            if (joinData != null)
            {
                foreach (var kvp in joinData)
                {
                    join.Add(kvp.Key);
                    join.Add(kvp.Value);
                }
            }

            this.Send(join);

            this.MessageDeserializer.OnDeserializedMessage += (message) =>
            {
                this.TotalMessagesReceived++;

                OnMessage?.Invoke(this, message);
            };
        }

        /// <summary>
        /// Send a message to the connected client without first having to construct a Message object.
        /// </summary>
        /// <param name="type"> The type of message to send. </param>
        /// <param name="parameters"> The values to place within the message to be sent. </param>
        /// <returns> A boolean representing whether the socket successfully sent the message. </returns>
        public bool Send(string type, params object[] parameters) => this.Send(new Message(type, parameters));

        /// <summary> Send a message to the connected client. </summary>
        /// <param name="message"> The message to send. </param>
        /// <returns> A boolean representing whether the socket successfully sent the message. </returns>
        public bool Send(Message message)
        {
            var serialized = new BinarySerializer().Serialize(message);

            if (this.Socket != null && this.Socket.Connected)
            {
                this.Socket.Send(serialized, SocketFlags.None, out var status);
                this.TotalMessagesSent++;
                this.TotalBytesSent += (ulong)serialized.Length;
                
                return (status == SocketError.Success);
            }

            return false;
        }

        /// <summary>
        /// A method to forcibly disconnect the current connection from the server.
        /// </summary>
        /// <param name="invokeOnDisconnect"> A boolean representing whether the <see cref="OnDisconnect"/> event should be subsequently fired. </param>
        public void Disconnect(bool invokeOnDisconnect = true)
        {
            if (this.Socket != null && this.Socket.Connected)
            {
                this.Socket.Disconnect(false);
                this.Socket.Dispose();
                this.Buffer = null;
                this.Stream = null;

                if (invokeOnDisconnect)
                    this.OnDisconnect?.Invoke(this, "The connection was forcibly terminated by local client.");
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
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

                this.TotalBytesReceived += (ulong)length;

                this.MessageDeserializer.AddBytes(received);
                this.Stream?.BeginRead(this.Buffer, 0, this.Buffer.Length, new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (SocketException ex)
            {
                this.OnDisconnect?.Invoke(this, $"The connection was forcibly reset by peer. (socket error: {ex.ErrorCode})");
                return;
            }
        }

        private ulong TotalMessagesSent { get; set; } = 0;
        private ulong TotalMessagesReceived { get; set; } = 0;

        private ulong TotalBytesReceived { get; set; } = 0;
        private ulong TotalBytesSent { get; set; } = 0;

        private ProxySocket Socket { get; set; }
        private Stream Stream { get; set; }
        private readonly BinaryDeserializer MessageDeserializer;
        private byte[] Buffer { get; set; } = new byte[ushort.MaxValue];

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}

namespace PlayerIOClient
{
    /// <summary>
    /// Message Deserialization
    /// </summary>
    internal class BinaryDeserializer
    {
        internal enum EnumPattern
        {
            STRING_SHORT_PATTERN = 0xC0,
            STRING_PATTERN = 0x0C,
            BYTE_ARRAY_SHORT_PATTERN = 0x40,
            BYTE_ARRAY_PATTERN = 0x10,
            UNSIGNED_LONG_SHORT_PATTERN = 0x38,
            UNSIGNED_LONG_PATTERN = 0x3C,
            LONG_SHORT_PATTERN = 0x30,
            LONG_PATTERN = 0x34,
            UNSIGNED_INT_SHORT_PATTERN = 0x80,
            UNSIGNED_INT_PATTERN = 0x08,
            INT_PATTERN = 0x04,
            DOUBLE_PATTERN = 0x03,
            FLOAT_PATTERN = 0x02,
            BOOLEAN_TRUE_PATTERN = 0x01,
            BOOLEAN_FALSE_PATTERN = 0x00,
            DOES_NOT_EXIST = -1
        }

        internal enum EnumState
        {
            INIT, HEADER, DATA
        }

        internal delegate void MessageDeserializedEventHandler(Message e);
        internal delegate void ValueDeserializedEventHandler(object value);

        public event MessageDeserializedEventHandler OnDeserializedMessage;
        internal event ValueDeserializedEventHandler OnDeserializedValue;

        internal EnumState State = EnumState.INIT;
        internal EnumPattern Pattern = EnumPattern.DOES_NOT_EXIST;

        internal MemoryStream _buffer;
        internal Message _message;

        internal int _length;
        internal int _partLength;

        public BinaryDeserializer()
        {
            _buffer = new MemoryStream();
            _message = null;
            _length = -1;
            _partLength = 0;

            OnDeserializedValue += (value) =>
            {
                if (_length == -1)
                {
                    _length = (int)value;
                }
                else
                {
                    if (_message == null)
                        _message = new Message((string)value);
                    else
                        _message.Add(value);

                    if (_length == _message.Count)
                    {
                        OnDeserializedMessage?.Invoke(_message);

                        _message = null;
                        _length = -1;
                    }
                }

                State = EnumState.INIT;
            };
        }

        public void AddBytes(byte[] input)
        {
            foreach (var value in input)
                this.DeserializeValue(value);
        }

        internal void DeserializeValue(byte value)
        {
            switch (State)
            {
                case EnumState.INIT:
                    Pattern = value.RetrieveFlagPattern();

                    switch (Pattern)
                    {
                        case EnumPattern.STRING_SHORT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern);

                            if (_partLength > 0)
                                State = EnumState.DATA;
                            else
                                OnDeserializedValue("");
                            break;
                        case EnumPattern.STRING_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.HEADER;
                            break;
                        case EnumPattern.BYTE_ARRAY_SHORT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern);

                            if (_partLength > 0)
                                State = EnumState.DATA;
                            else
                                OnDeserializedValue(new byte[] { });
                            break;
                        case EnumPattern.BYTE_ARRAY_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.HEADER;
                            break;
                        case EnumPattern.UNSIGNED_INT_SHORT_PATTERN:
                            OnDeserializedValue(value.RetrievePartLength(Pattern));
                            break;
                        case EnumPattern.UNSIGNED_INT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.INT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.UNSIGNED_LONG_SHORT_PATTERN:
                            _partLength = 1;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.UNSIGNED_LONG_PATTERN:
                            _partLength = 6;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.LONG_SHORT_PATTERN:
                            _partLength = 1;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.LONG_PATTERN:
                            _partLength = 6;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.DOUBLE_PATTERN:
                            _partLength = 8;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.FLOAT_PATTERN:
                            _partLength = 4;
                            State = EnumState.DATA;
                            break;
                        case EnumPattern.BOOLEAN_TRUE_PATTERN:
                            OnDeserializedValue(true);
                            break;
                        case EnumPattern.BOOLEAN_FALSE_PATTERN:
                            OnDeserializedValue(false);
                            break;
                    }
                    break;

                case EnumState.HEADER:
                    _buffer.WriteByte(value);

                    if (_buffer.Position == _partLength)
                    {
                        var buffer = _buffer.ToArray().Reverse().ToArray();
                        var length = new List<byte>(4) { 0, 0, 0, 0 }.Select((b, index) => index <= _partLength - 1 ? buffer[index] : (byte)0);

                        _partLength = BitConverter.ToInt32(length.ToArray(), 0);
                        State = EnumState.DATA;

                        _buffer.Position = 0;
                    }
                    break;
                case EnumState.DATA:
                    _buffer.WriteByte(value);

                    if (_buffer.Position == _partLength)
                    {
                        var buffer = _buffer.ToArray();
                        var length = new List<byte>(8) { 0, 0, 0, 0, 0, 0, 0, 0 }.Select((v, index) => index <= _partLength - 1 ? buffer[index] : (byte)0);

                        Array.Reverse(buffer, 0, _partLength);

                        switch (Pattern)
                        {
                            case EnumPattern.STRING_SHORT_PATTERN:
                            case EnumPattern.STRING_PATTERN:
                                OnDeserializedValue(Encoding.UTF8.GetString(_buffer.ToArray()));
                                break;
                            case EnumPattern.UNSIGNED_INT_PATTERN:
                                OnDeserializedValue(BitConverter.ToUInt32(length.ToArray(), 0));
                                break;
                            case EnumPattern.INT_PATTERN:
                                OnDeserializedValue(BitConverter.ToInt32(length.ToArray(), 0));
                                break;
                            case EnumPattern.UNSIGNED_LONG_SHORT_PATTERN:
                            case EnumPattern.UNSIGNED_LONG_PATTERN:
                                OnDeserializedValue(BitConverter.ToUInt64(length.ToArray(), 0));
                                break;
                            case EnumPattern.LONG_SHORT_PATTERN:
                            case EnumPattern.LONG_PATTERN:
                                OnDeserializedValue(BitConverter.ToInt64(length.ToArray(), 0));
                                break;
                            case EnumPattern.DOUBLE_PATTERN:
                                OnDeserializedValue(BitConverter.ToDouble(length.ToArray(), 0));
                                break;
                            case EnumPattern.FLOAT_PATTERN:
                                OnDeserializedValue(BitConverter.ToSingle(length.ToArray(), 0));
                                break;
                            case EnumPattern.BYTE_ARRAY_SHORT_PATTERN:
                            case EnumPattern.BYTE_ARRAY_PATTERN:
                                OnDeserializedValue(_buffer.ToArray());
                                break;
                        }

                        _buffer = new MemoryStream();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Message Serialization
    /// </summary>
    internal class BinarySerializer
    {
        internal static MemoryStream _buffer = new MemoryStream();

        public byte[] Serialize(Message message)
        {
            byte[] output;

            this.SerializeValue(message.Count);
            this.SerializeValue(message.Type);

            foreach (var value in message)
                this.SerializeValue(value);

            output = _buffer.ToArray();
            _buffer = new MemoryStream();

            return output;
        }

        private void SerializeValue(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.String:
                    Writer.WriteTagWithLength(Encoding.UTF8.GetBytes(value as string).Length, EnumPattern.STRING_SHORT_PATTERN, EnumPattern.STRING_PATTERN);
                    Writer.Write(Encoding.UTF8.GetBytes(value as string));
                    break;
                case TypeCode.Int32:
                    Writer.WriteTagWithLength((int)value, EnumPattern.UNSIGNED_INT_SHORT_PATTERN, EnumPattern.INT_PATTERN);
                    break;
                case TypeCode.UInt32:
                    Writer.WriteBottomPatternAndBytes(EnumPattern.UNSIGNED_INT_PATTERN, LittleEndianToNetworkOrderBitConverter.GetBytes((uint)value));
                    break;
                case TypeCode.Int64:
                    Writer.WriteLongPattern(EnumPattern.LONG_SHORT_PATTERN, EnumPattern.LONG_PATTERN, LittleEndianToNetworkOrderBitConverter.GetBytes((long)value));
                    break;
                case TypeCode.UInt64:
                    Writer.WriteLongPattern(EnumPattern.UNSIGNED_LONG_SHORT_PATTERN, EnumPattern.UNSIGNED_LONG_PATTERN, LittleEndianToNetworkOrderBitConverter.GetBytes((ulong)value));
                    break;
                case TypeCode.Double:
                    Writer.Write(EnumPattern.DOUBLE_PATTERN);
                    Writer.Write(LittleEndianToNetworkOrderBitConverter.GetBytes((double)value));
                    break;
                case TypeCode.Single:
                    Writer.Write(EnumPattern.FLOAT_PATTERN);
                    Writer.Write(LittleEndianToNetworkOrderBitConverter.GetBytes((float)value));
                    break;
                case TypeCode.Boolean:
                    Writer.Write((bool)value ? EnumPattern.BOOLEAN_TRUE_PATTERN : EnumPattern.BOOLEAN_FALSE_PATTERN);
                    break;
                case TypeCode.Object:
                    if (!(value is byte[]))
                        break;

                    var array = (byte[])value;

                    Writer.WriteTagWithLength(array.Length, EnumPattern.BYTE_ARRAY_SHORT_PATTERN, EnumPattern.BYTE_ARRAY_PATTERN);
                    Writer.Write(array);
                    break;
            }
        }

        private static class Writer
        {
            internal static void WriteTagWithLength(int length, EnumPattern topPattern, EnumPattern bottomPattern)
            {
                if (length > 63 || length < 0)
                {
                    var bytes = LittleEndianToNetworkOrderBitConverter.GetBytes(length);
                    WriteBottomPatternAndBytes(bottomPattern, bytes);
                }
                else
                {
                    _buffer.WriteByte((byte)((int)topPattern | length));
                }
            }

            internal static void WriteBottomPatternAndBytes(EnumPattern pattern, byte[] bytes)
            {
                var counter = bytes[0] != 0 ? 3 : bytes[1] != 0 ? 2 : bytes[2] != 0 ? 1 : 0;

                _buffer.WriteByte((byte)((int)pattern | counter));
                _buffer.Write(bytes, bytes.Length - counter - 1, counter + 1);
            }

            internal static void WriteLongPattern(EnumPattern shortPattern, EnumPattern longPattern, byte[] bytes)
            {
                var counter = 0;
                for (var nc = 0; nc != 7; nc++)
                {
                    if (bytes[nc] != 0)
                    {
                        counter = 7 - nc;
                        break;
                    }
                }

                if (counter > 3)
                    _buffer.WriteByte((byte)((byte)longPattern | counter - 4));
                else
                    _buffer.WriteByte((byte)((byte)shortPattern | counter));

                _buffer.Write(bytes, bytes.Length - counter - 1, counter + 1);
            }

            internal static void Write(EnumPattern pattern) => Write((byte)pattern);

            internal static void Write(byte value)
            {
                _buffer.WriteByte(value);
            }

            internal static void Write(byte[] value)
            {
                _buffer.Write(value, 0, value.Length);
            }
        }
    }

    internal static class PatternHelper
    {
        internal static EnumPattern RetrieveFlagPattern(this byte input)
        {
            foreach (var pattern in ((EnumPattern[])Enum.GetValues(typeof(EnumPattern))).OrderByDescending(p => p))
                if ((input & (byte)pattern) == (byte)pattern)
                    return pattern;

            return EnumPattern.DOES_NOT_EXIST;
        }

        internal static int RetrievePartLength(this byte input, EnumPattern pattern) => input & ~(byte)pattern;
    }

    internal static class LittleEndianToNetworkOrderBitConverter
    {
        public static byte[] GetBytes(int value) => ReverseArray(BitConverter.GetBytes(value), 4, 0);
        public static byte[] GetBytes(uint value) => ReverseArray(BitConverter.GetBytes(value), 4, 0);
        public static byte[] GetBytes(long value) => ReverseArray(BitConverter.GetBytes(value), 8, 0);
        public static byte[] GetBytes(ushort value) => ReverseArray(BitConverter.GetBytes(value), 2, 0);
        public static byte[] GetBytes(ulong value) => ReverseArray(BitConverter.GetBytes(value), 8, 0);
        public static byte[] GetBytes(float value) => ReverseArray(BitConverter.GetBytes(value), 4, 0);
        public static byte[] GetBytes(double value) => ReverseArray(BitConverter.GetBytes(value), 8, 0);

        public static int ToInt32(byte[] value, int startIndex, int length) => BitConverter.ToInt32(ReverseArray(value, length, startIndex), startIndex);
        public static uint ToUInt32(byte[] value, int startIndex, int length) => BitConverter.ToUInt32(ReverseArray(value, length, startIndex), startIndex);
        public static long ToInt64(byte[] value, int startIndex, int length) => BitConverter.ToInt64(ReverseArray(value, length, startIndex), startIndex);
        public static ulong ToUInt64(byte[] value, int startIndex, int length) => BitConverter.ToUInt64(ReverseArray(value, length, startIndex), startIndex);
        public static float ToSingle(byte[] value, int startIndex, int length) => BitConverter.ToSingle(ReverseArray(value, length, startIndex), startIndex);
        public static double ToDouble(byte[] value, int startIndex, int length) => BitConverter.ToDouble(ReverseArray(value, length, startIndex), startIndex);

        private static byte[] ReverseArray(byte[] input, int length, int startIndex)
        {
            Array.Reverse(input, startIndex, length);
            return input;
        }
    }
}