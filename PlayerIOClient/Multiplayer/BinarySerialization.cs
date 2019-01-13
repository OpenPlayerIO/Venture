using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static PlayerIOClient.BinaryDeserializer;

namespace PlayerIOClient
{
    internal class BinaryDeserializer
    {
        internal enum EnumPattern
        {
            STRING_SHORT_PATTERN = 0xC0, STRING_PATTERN = 0x0C, BYTE_ARRAY_SHORT_PATTERN = 0x40, BYTE_ARRAY_PATTERN = 0x10,
            UNSIGNED_LONG_SHORT_PATTERN = 0x38, UNSIGNED_LONG_PATTERN = 0x3C, LONG_SHORT_PATTERN = 0x30, LONG_PATTERN = 0x34,
            UNSIGNED_INT_SHORT_PATTERN = 0x80, UNSIGNED_INT_PATTERN = 0x08, INT_PATTERN = 0x04, DOUBLE_PATTERN = 0x03,
            FLOAT_PATTERN = 0x02, BOOLEAN_TRUE_PATTERN = 0x01, BOOLEAN_FALSE_PATTERN = 0x00, DOES_NOT_EXIST = -1
        }

        internal enum EnumState
        {
            Init, Header, Data
        }

        internal delegate void MessageDeserializedEventHandler(Message e);
        internal delegate void ValueDeserializedEventHandler(object value);

        public event MessageDeserializedEventHandler OnDeserializedMessage;
        internal event ValueDeserializedEventHandler OnDeserializedValue;

        internal EnumState State = EnumState.Init;
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

            OnDeserializedValue += (value) => {
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

                State = EnumState.Init;
            };
        }

        public void AddBytes(byte[] input)
        {
            foreach (var value in input)
                DeserializeValue(value);
        }

        internal void DeserializeValue(byte value)
        {
            switch (State)
            {
                case EnumState.Init:
                    Pattern = value.RetrieveFlagPattern();

                    switch (Pattern)
                    {
                        case EnumPattern.STRING_SHORT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern);

                            if (_partLength > 0)
                                State = EnumState.Data;
                            else
                                OnDeserializedValue("");
                            break;
                        case EnumPattern.STRING_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.Header;
                            break;
                        case EnumPattern.BYTE_ARRAY_SHORT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern);

                            if (_partLength > 0)
                                State = EnumState.Data;
                            else
                                OnDeserializedValue(new byte[] { });
                            break;
                        case EnumPattern.BYTE_ARRAY_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.Header;
                            break;
                        case EnumPattern.UNSIGNED_INT_SHORT_PATTERN:
                            OnDeserializedValue(value.RetrievePartLength(Pattern));
                            break;
                        case EnumPattern.UNSIGNED_INT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.INT_PATTERN:
                            _partLength = value.RetrievePartLength(Pattern) + 1;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.UNSIGNED_LONG_SHORT_PATTERN:
                            _partLength = 1;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.UNSIGNED_LONG_PATTERN:
                            _partLength = 6;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.LONG_SHORT_PATTERN:
                            _partLength = 1;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.LONG_PATTERN:
                            _partLength = 6;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.DOUBLE_PATTERN:
                            _partLength = 8;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.FLOAT_PATTERN:
                            _partLength = 4;
                            State = EnumState.Data;
                            break;
                        case EnumPattern.BOOLEAN_TRUE_PATTERN:
                            OnDeserializedValue(true);
                            break;
                        case EnumPattern.BOOLEAN_FALSE_PATTERN:
                            OnDeserializedValue(false);
                            break;
                    }
                    break;

                case EnumState.Header:
                    _buffer.WriteByte(value);

                    if (_buffer.Position == _partLength)
                    {
                        var buffer = _buffer.ToArray().Reverse().ToArray();
                        var length = new List<byte>(4) { 0, 0, 0, 0 }.Select((b, index) => index <= _partLength - 1 ? buffer[index] : (byte)0);

                        _partLength = BitConverter.ToInt32(length.ToArray(), 0);
                        State = EnumState.Data;

                        _buffer.Position = 0;
                    }
                    break;
                case EnumState.Data:
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

    internal class BinarySerializer
    {
        internal MemoryStream _buffer = new MemoryStream();

        public byte[] Serialize(Message message)
        {
            byte[] output;

            SerializeValue(message.Count);
            SerializeValue(message.Type);

            foreach (var value in message)
                SerializeValue(value);

            output = _buffer.ToArray();
            _buffer = new MemoryStream();

            return output;
        }

        private void SerializeValue(object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.String:
                    _buffer.WriteTagWithLength(Encoding.UTF8.GetBytes(value as string).Length, EnumPattern.STRING_SHORT_PATTERN, EnumPattern.STRING_PATTERN);
                    _buffer.Write(Encoding.UTF8.GetBytes(value as string));
                    break;
                case TypeCode.Int32:
                    _buffer.WriteTagWithLength((int)value, EnumPattern.UNSIGNED_INT_SHORT_PATTERN, EnumPattern.INT_PATTERN);
                    break;
                case TypeCode.UInt32:
                    _buffer.WriteBottomPatternAndBytes(EnumPattern.UNSIGNED_INT_PATTERN, LittleEndianToNetworkOrderBitConverter.GetBytes((uint)value));
                    break;
                case TypeCode.Int64:
                    _buffer.WriteLongPattern(EnumPattern.LONG_SHORT_PATTERN, EnumPattern.LONG_PATTERN, LittleEndianToNetworkOrderBitConverter.GetBytes((long)value));
                    break;
                case TypeCode.UInt64:
                    _buffer.WriteLongPattern(EnumPattern.UNSIGNED_LONG_SHORT_PATTERN, EnumPattern.UNSIGNED_LONG_PATTERN, LittleEndianToNetworkOrderBitConverter.GetBytes((ulong)value));
                    break;
                case TypeCode.Double:
                    _buffer.Write(EnumPattern.DOUBLE_PATTERN);
                    _buffer.Write(LittleEndianToNetworkOrderBitConverter.GetBytes((double)value));
                    break;
                case TypeCode.Single:
                    _buffer.Write(EnumPattern.FLOAT_PATTERN);
                    _buffer.Write(LittleEndianToNetworkOrderBitConverter.GetBytes((float)value));
                    break;
                case TypeCode.Boolean:
                    _buffer.Write((bool)value ? EnumPattern.BOOLEAN_TRUE_PATTERN : EnumPattern.BOOLEAN_FALSE_PATTERN);
                    break;
                case TypeCode.Object:
                    if (!(value is byte[]))
                        break;

                    var array = (byte[])value;

                    _buffer.WriteTagWithLength(array.Length, EnumPattern.BYTE_ARRAY_SHORT_PATTERN, EnumPattern.BYTE_ARRAY_PATTERN);
                    _buffer.Write(array);
                    break;
            }
        }
    }

    internal static class WriterExtensions
    {
        internal static void WriteTagWithLength(this Stream stream, int length, EnumPattern topPattern, EnumPattern bottomPattern)
        {
            if (length > 63 || length < 0)
            {
                var bytes = LittleEndianToNetworkOrderBitConverter.GetBytes(length);
                stream.WriteBottomPatternAndBytes(bottomPattern, bytes);
            }
            else
            {
                stream.WriteByte((byte)((int)topPattern | length));
            }
        }

        internal static void WriteBottomPatternAndBytes(this Stream stream, EnumPattern pattern, byte[] bytes)
        {
            var counter = bytes[0] != 0 ? 3 : bytes[1] != 0 ? 2 : bytes[2] != 0 ? 1 : 0;

            stream.WriteByte((byte)((int)pattern | counter));
            stream.Write(bytes, bytes.Length - counter - 1, counter + 1);
        }

        internal static void WriteLongPattern(this Stream stream, EnumPattern shortPattern, EnumPattern longPattern, byte[] bytes)
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
                stream.WriteByte((byte)((byte)longPattern | counter - 4));
            else
                stream.WriteByte((byte)((byte)shortPattern | counter));

            stream.Write(bytes, bytes.Length - counter - 1, counter + 1);
        }

        internal static void Write(this Stream stream, EnumPattern pattern) => stream.Write((byte)pattern);

        internal static void Write(this Stream stream, byte value)
        {
            stream.WriteByte(value);
        }

        internal static void Write(this Stream stream, byte[] value)
        {
            stream.Write(value, 0, value.Length);
        }
    }

    internal static class LittleEndianToNetworkOrderBitConverter
    {
        public static byte[] GetBytes(int value) => Reverse(BitConverter.GetBytes(value), 4, 0);
        public static byte[] GetBytes(uint value) => Reverse(BitConverter.GetBytes(value), 4, 0);
        public static byte[] GetBytes(long value) => Reverse(BitConverter.GetBytes(value), 8, 0);
        public static byte[] GetBytes(ushort value) => Reverse(BitConverter.GetBytes(value), 2, 0);
        public static byte[] GetBytes(ulong value) => Reverse(BitConverter.GetBytes(value), 8, 0);
        public static byte[] GetBytes(float value) => Reverse(BitConverter.GetBytes(value), 4, 0);
        public static byte[] GetBytes(double value) => Reverse(BitConverter.GetBytes(value), 8, 0);

        public static int ToInt32(byte[] value, int startIndex, int length) => BitConverter.ToInt32(value.Reverse(length, startIndex), startIndex);
        public static uint ToUInt32(byte[] value, int startIndex, int length) => BitConverter.ToUInt32(value.Reverse(length, startIndex), startIndex);
        public static long ToInt64(byte[] value, int startIndex, int length) => BitConverter.ToInt64(value.Reverse(length, startIndex), startIndex);
        public static ulong ToUInt64(byte[] value, int startIndex, int length) => BitConverter.ToUInt64(value.Reverse(length, startIndex), startIndex);
        public static float ToSingle(byte[] value, int startIndex, int length) => BitConverter.ToSingle(value.Reverse(length, startIndex), startIndex);
        public static double ToDouble(byte[] value, int startIndex, int length) => BitConverter.ToDouble(value.Reverse(length, startIndex), startIndex);

        private static byte[] Reverse(this byte[] input, int length, int startIndex)
        {
            Array.Reverse(input, startIndex, length);
            return input;
        }
    }
}