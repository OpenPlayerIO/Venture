using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PlayerIOClient
{
    public class DatabaseArray : DatabaseObject, IEnumerable<object>
    {
        internal DatabaseArray(BigDB owner, string table, string key, string version, List<ObjectProperty> properties) : base(owner, table, key, version, properties)
        {
        }

        public DatabaseArray() : base(null, string.Empty, string.Empty, string.Empty, new List<ObjectProperty>())
        {
        }

        public new object[] Values => this.Properties.Values.ToArray();
        public object this[uint index] => index <= this.Values.Length - 1 ? this.Values[index] ?? null : throw new IndexOutOfRangeException(nameof(index));

        public DatabaseArray Set(uint index, object value) => this.SetProperty(index.ToString(), value) as DatabaseArray;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public DatabaseArray Add(object value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given string value to the array. </summary>
        public DatabaseArray Add(string value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given int value to the array. </summary>
        public DatabaseArray Add(int value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given uint value to the array. </summary>
        public DatabaseArray Add(uint value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given long value to the array. </summary>
        public DatabaseArray Add(long value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given ulong value to the array. </summary>
        public DatabaseArray Add(ulong value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given float value to the array. </summary>
        public DatabaseArray Add(float value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given double value to the array. </summary>
        public DatabaseArray Add(double value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given boolean value to the array. </summary>
        public DatabaseArray Add(bool value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given byte array value to the array. </summary>
        public DatabaseArray Add(byte[] value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given date time value to the array. </summary>
        public DatabaseArray Add(DateTime value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given object to the array. </summary>
        public DatabaseArray Add(DatabaseObject value) => this.Set((uint)this.Properties.Count, value);

        /// <summary> Add the given array to the array. </summary>
        public DatabaseArray Add(DatabaseArray value) => this.Set((uint)this.Properties.Count, value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override DatabaseObject SetProperty(string index, object value)
        {
            if (!int.TryParse(index, out int i))
                throw new Exception("You must specify the index as an integer.");

            for (var j = this.Properties.Count; j < i; j++)
                base.SetProperty(j.ToString(), null);

            return base.SetProperty(index, value);
        }

        public bool GetBool(uint index) => this.GetBool(index.ToString());
        public bool GetBool(uint index, bool defaultValue) => this.GetBool(index.ToString(), defaultValue);

        public byte[] GetBytes(uint index) => this.GetBytes(index.ToString());
        public byte[] GetBytes(uint index, byte[] defaultValue) => this.GetBytes(index.ToString(), defaultValue);

        public double GetDouble(uint index) => this.GetDouble(index.ToString());
        public double GetDouble(uint index, double defaultValue) => this.GetDouble(index.ToString(), defaultValue);

        public float GetFloat(uint index) => this.GetFloat(index.ToString());
        public float GetFloat(uint index, float defaultValue) => this.GetFloat(index.ToString(), defaultValue);

        public int GetInt(uint index) => this.GetInt(index.ToString());
        public int GetInt(uint index, int defaultValue) => this.GetInt(index.ToString(), defaultValue);

        public uint GetUInt(uint index) => this.GetUInt(index.ToString());
        public uint GetUInt(uint index, uint defaultValue) => this.GetUInt(index.ToString(), defaultValue);

        public long GetLong(uint index) => this.GetLong(index.ToString());
        public long GetLong(uint index, long defaultValue) => this.GetLong(index.ToString(), defaultValue);

        public string GetString(uint index) => this.GetString(index.ToString());
        public string GetString(uint index, string defaultValue) => this.GetString(index.ToString(), defaultValue);

        public DatabaseObject GetObject(uint index) => this.GetObject(index.ToString());
        public DatabaseArray GetArray(uint index) => this.GetArray(index.ToString());

        public new IEnumerator<object> GetEnumerator() => ((IEnumerable<object>)this.Values).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<object>)this.Values).GetEnumerator();
    }
}