using System;
using System.Collections.Generic;
using Tson.NET;

namespace PlayerIOClient
{
    public class DatabaseObject
    {
        internal DatabaseObject(BigDB owner, string table, string key, string version, List<ObjectProperty> properties)
        {
            this.Owner = owner;
            this.Table = table;
            this.Key = key;
            this.Version = version;
            this.Properties = (Dictionary<string, object>)BigDBExtensions.ToDictionary(properties);
        }

        /// <summary>
        /// The name of the table the object belongs to.
        /// </summary>
        public string Table { get; }

        /// <summary>
        /// The key of the object.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The version of the object, incremented every save.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The properties of the object.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        public object this[string prop] => Properties.ContainsKey(prop) ? Properties[prop] : null;
        public object this[string prop, Type type] => Get(prop, type);
        private object Get(string prop, Type type)
        {
            if (!Properties.ContainsKey(prop) || Properties[prop] == null)
                throw new PlayerIOError(ErrorCode.GeneralError, (GetType() == typeof(DatabaseArray) ? "The array does not have an entry at: " : "Property does not exist: ") + prop);

            if (Properties[prop].GetType() != type)
                throw new PlayerIOError(ErrorCode.GeneralError, $"No property found with the type '{ type.Name }'.");

            return Properties[prop];
        }

        public virtual DatabaseObject Set(string property, object value)
        {
            var allowedTypes = new List<Type>()
            {
                typeof(string), typeof(int),    typeof(uint),
                typeof(long),   typeof(ulong),  typeof(float),
                typeof(double), typeof(bool),   typeof(byte[]),
                typeof(DatabaseObject), typeof(DatabaseArray)
            };

            if (value != null && !allowedTypes.Contains(value.GetType()))
                throw new PlayerIOError(ErrorCode.GeneralError, $"The type '{ value.GetType().Name }' is not allowed.");

            this.Properties.Add(property, value);
            return this;
        }

        public bool GetBool(string prop) => (bool)this[prop, typeof(bool)];
        public bool GetBool(string prop, bool defaultValue) => this[prop] is bool value ? value : defaultValue;

        public byte[] GetBytes(string prop) => (byte[])this[prop, typeof(byte[])];
        public byte[] GetBytes(string prop, byte[] defaultValue) => this[prop] is byte[] value ? value : defaultValue;

        public double GetDouble(string prop) => (double)this[prop, typeof(double)];
        public double GetDouble(string prop, double defaultValue) => this[prop] is double value ? value : defaultValue;

        public float GetFloat(string prop) => (float)this[prop, typeof(float)];
        public float GetFloat(string prop, float defaultValue) => this[prop] is float value ? value : defaultValue;

        public int GetInt(string prop) => (int)this[prop, typeof(int)];
        public int GetInt(string prop, int defaultValue) => this[prop] is int value ? value : defaultValue;

        public uint GetUInt(string prop) => (uint)this[prop, typeof(uint)];
        public uint GetUInt(string prop, uint defaultValue) => this[prop] is uint value ? value : defaultValue;

        public long GetLong(string prop) => (long)this[prop, typeof(long)];
        public long GetLong(string prop, long defaultValue) => this[prop] is long value ? value : defaultValue;

        public string GetString(string prop) => (string)this[prop, typeof(string)];
        public string GetString(string prop, string defaultValue) => this[prop] is string value ? value : defaultValue;

        public DateTime GetDateTime(string prop) => (DateTime)this[prop, typeof(DateTime)];
        public DateTime GetDateTime(string prop, DateTime defaultValue) => this[prop] is DateTime value ? value : defaultValue;

        public DatabaseObject GetObject(string prop) => (DatabaseObject)this[prop, typeof(DatabaseObject)];
        public DatabaseObject GetObject(string prop, DatabaseObject defaultValue) => this[prop] is DatabaseObject value ? value : defaultValue;

        public DatabaseArray GetArray(string prop) => (DatabaseArray)this[prop, typeof(DatabaseArray)];
        public DatabaseArray GetArray(string prop, DatabaseArray defaultValue) => this[prop] is DatabaseArray value ? value : defaultValue;

        public override string ToString()
        {
            return TsonConvert.SerializeObject(this.Properties, Formatting.Indented);
        }

        internal BigDB Owner { get; }
    }

    internal static class BigDBExtensions
    {
        internal static object ToDictionary(object input)
        {
            var dictionary = new Dictionary<string, object>();

            switch (input)
            {
                case List<ObjectProperty> databaseObject:
                    foreach (var property in databaseObject)
                        dictionary.Add(property.Name, ToDictionary(property.Value));
                    break;
                case ValueObject valueObject:
                    var value = Value(valueObject);

                    if (value is List<ObjectProperty> object_properties)
                    {
                        foreach (var property in object_properties)
                            dictionary.Add(property.Name, ToDictionary(property.Value));
                    }
                    else if (value is List<ArrayProperty> array_properties)
                    {
                        var array = new object[array_properties.Count];

                        for (var i = 0; i < array_properties.Count; i++)
                            array[i] = ToDictionary(array_properties[i].Value);

                        return array;
                    }
                    else
                    {
                        return ToDictionary(value);
                    }

                    break;
                case null:
                    return null;
                default:
                    return input;
            }

            return dictionary;
        }

        internal static object Value(this ArrayProperty property) => Value(property.Value);
        internal static object Value(this ObjectProperty property) => Value(property.Value);
        internal static object Value(ValueObject value)
        {
            switch (value.ValueType)
            {
                case ValueType.String:
                    return value.String;
                case ValueType.Int:
                    return value.Int;
                case ValueType.UInt:
                    return value.UInt;
                case ValueType.Long:
                    return value.Long;
                case ValueType.Bool:
                    return value.Bool;
                case ValueType.Float:
                    return value.Float;
                case ValueType.Double:
                    return value.Double;
                case ValueType.ByteArray:
                    return value.ByteArray;
                case ValueType.DateTime:
                    return new DateTime(1970, 1, 1).AddMilliseconds(value.DateTime);
                case ValueType.Array:
                    return value.ArrayProperties;
                case ValueType.Obj:
                    return value.ObjectProperties;
                default: return null;
            }
        }


    }
}
