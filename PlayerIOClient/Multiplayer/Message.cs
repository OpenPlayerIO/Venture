using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PlayerIOClient
{
    public class Message : IEnumerable<object>
    {
        public string Type { get; internal set; }
        public List<object> Values { get; internal set; } = new List<object>();
        public int Count => this.Values.Count;

        public Message(string type)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));

            this.Type = type;
        }

        public Message(string type, params object[] parameters)
        {
            if (string.IsNullOrEmpty(type))
                throw new ArgumentNullException(nameof(type));

            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            this.Type = type;
            this.Add(parameters);
        }

        public static Message Create(string type, params object[] parameters)
        {
            if (string.IsNullOrEmpty(type))
                throw new Exception("You must specify a type for the PlayerIO message.");

            var message = new Message(type);
            message.Add(parameters);

            return message;
        }

        public IEnumerator<object> GetEnumerator() => this.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public override string ToString()
        {
            var sb = new StringBuilder("");

            sb.AppendLine($"  msg.Type= {this.Type}, {this.Values.Count} entries");

            for (var i = 0; i < this.Values.Count; i++)
                sb.AppendLine($"  msg[{i}] = {this.Values[i]}  ({this.Values[i].GetType().Name})");

            return sb.ToString();
        }

        public object this[uint index] => this.Values[(int)index];

        public string GetString(uint index) => (string)this[index];
        public byte[] GetByteArray(uint index) => (byte[])this[index];
        public bool GetBoolean(uint index) => (bool)this[index];
        public double GetDouble(uint index) => (double)this[index];
        public float GetFloat(uint index) => (float)this[index];
        public int GetInteger(uint index) => (int)this[index];
        public int GetInt(uint index) => (int)this[index];
        public uint GetUInt(uint index) => (uint)this[index];
        public uint GetUnsignedInteger(uint index) => (uint)this[index];
        public long GetLong(uint index) => (long)this[index];
        public ulong GetULong(uint index) => (ulong)this[index];
        public ulong GetUnsignedLong(uint index) => (ulong)this[index];

        public void Add(string value) => Add(value);
        public void Add(int value) => Add(value);
        public void Add(uint value) => Add(value);
        public void Add(long value) => Add(value);
        public void Add(ulong value) => Add(value);
        public void Add(byte[] value) => Add(value);
        public void Add(float value) => Add(value);
        public void Add(double value) => Add(value);
        public void Add(bool value) => Add(value);

        public void Add(params object[] parameters)
        {
            if (parameters.Length == 0)
                return;

            var allowedTypes = new List<Type>() {
                typeof(string), typeof(int),    typeof(uint),
                typeof(long),   typeof(ulong),  typeof(float),
                typeof(double), typeof(bool),   typeof(byte[])
            };

            foreach (var value in parameters)
            {
                if (value == null)
                    throw new Exception("PlayerIO messages do not support null objects.");

                if (!allowedTypes.Contains(value.GetType()))
                    throw new Exception($"PlayerIO messages do not support objects of type '{value.GetType()}'");

                this.Values.Add(value);
            }
        }
    }
}