using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Tson.NET;

namespace PlayerIOClient
{
    public partial class DatabaseObject : IDictionary<string, object>
    {
        public DatabaseObject(BigDB owner, string table, string key, string version, List<ObjectProperty> properties)
        {
            this.Owner = owner;
            this.Table = table;
            this.Key = key;
            this.Version = version;
            this.Properties = (BigDBExtensions.FromDictionary(BigDBExtensions.ToDictionary(properties)) as DatabaseObject).Properties;
        }

        public DatabaseObject()
        {
            this.Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// The name of the table the object belongs to.
        /// </summary>
        public string Table { get; internal set; }

        /// <summary>
        /// The key of the object.
        /// </summary>
        public string Key { get; internal set; }

        /// <summary>
        /// The version of the object, incremented every save.
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// The properties of the object.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        public ICollection<object> Values => this.Properties.Values;
        public ICollection<string> Keys => this.Properties.Keys;

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
            if (property.Contains("."))
                throw new InvalidOperationException("You must not include periods within the property name.");

            var allowedTypes = new List<Type>()
            {
                typeof(string), typeof(int),    typeof(uint),
                typeof(long),   typeof(ulong),  typeof(float),
                typeof(double), typeof(bool),   typeof(byte[]),
                typeof(DateTime), typeof(DatabaseObject), typeof(DatabaseArray)
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

        /// <summary>
        /// Check whether this object contains the specified property.
        /// </summary>
        /// <param name="property"> The name of the property. </param>
        /// <returns> If the object contains the property, returns true. </returns>
        public bool ContainsKey(string property) => this.Properties.ContainsKey(property);

        /// <summary>
        /// Removes a property from this object.
        /// </summary>
        /// <param name="property"> The property to remove. </param>
        /// <returns> If the property has been successfully removed, returns true.  </returns>
        public bool Remove(string property) => this.Properties.Remove(property);

        /// <summary>
        /// Removes all properties on this object.
        /// </summary>
        public void Clear() => Properties.Clear();

        /// <summary>
        /// The amount of properties within this object.
        /// </summary>
        public int Count => Properties.Count;

        /// <summary>
        /// Return a TSON string of the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TsonConvert.SerializeObject(this.Properties, Formatting.Indented);
        }

        internal BigDB Owner { get; set; }
    }

    public partial class DatabaseObject
    {
        [EditorBrowsable(EditorBrowsableState.Never)] public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public bool Contains(KeyValuePair<string, object> item) => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public bool Remove(KeyValuePair<string, object> item) => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public bool TryGetValue(string key, out object value) => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public void Add(KeyValuePair<string, object> item) => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public void Add(string key, object value) => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");
        [EditorBrowsable(EditorBrowsableState.Never)] public bool IsReadOnly => throw new InvalidOperationException("The requested method is disabled, please use the public methods instead.");

        object IDictionary<string, object>.this[string key] { get => this.Properties[key]; set => this.Properties[key] = value; }
    }

    public static class BigDBExtensions
    {
        public static object ToDictionary(object input)
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

                case null: return null;
                default: return input;
            }

            return dictionary;
        }

        public static List<ObjectProperty> FromDatabaseObject(DatabaseObject input)
        {
            var model = new List<ObjectProperty>();

            foreach (var kvp in input.Properties.Where(kvp => kvp.Value != null))
            {
                if (kvp.Value.GetType() == typeof(DatabaseObject))
                {
                    model.Add(new ObjectProperty() { Name = kvp.Key, Value = new ValueObject() { ValueType = ValueType.Object, ObjectProperties = FromDatabaseObject(kvp.Value as DatabaseObject) } });
                }
                else if (kvp.Value.GetType() == typeof(DatabaseArray))
                {
                    model.Add(new ObjectProperty() { Name = kvp.Key, Value = new ValueObject() { ValueType = ValueType.Array, ArrayProperties = FromDatabaseArray(kvp.Value as DatabaseArray) } });
                }
                else
                {
                    model.Add(new ObjectProperty() { Name = kvp.Key, Value = Create(kvp.Value) });
                }
            }

            return model;
        }

        public static List<ArrayProperty> FromDatabaseArray(DatabaseArray input)
        {
            var model = new List<ArrayProperty>();

            for (var i = 0; i < input.Values.Length; i++)
            {
                var value = input.Values[i];

                if (value is DatabaseArray array)
                {
                    model.AddRange(FromDatabaseArray(array));
                }
                else if (value is DatabaseObject obj)
                {
                    model.Add(new ArrayProperty() { Index = i, Value = new ValueObject() { ValueType = ValueType.Object, ObjectProperties = FromDatabaseObject(obj) } });
                }
                else
                {
                    model.Add(new ArrayProperty() { Index = i, Value = Create(value) });
                }
            }

            return model;
        }

        public static object FromDictionary(object input)
        {
            var model = new DatabaseObject();

            if (input is Dictionary<string, object>)
            {
                foreach (var kvp in input as Dictionary<string, object>)
                {
                    if (kvp.Value is Dictionary<string, object>)
                    {
                        model.Set(kvp.Key, FromDictionary(kvp.Value as Dictionary<string, object>));
                    }
                    else if (kvp.Value is object[])
                    {
                        var array = new DatabaseArray();

                        foreach (var value in kvp.Value as object[])
                        {
                            array.Add(FromDictionary(value));
                        }

                        model.Set(kvp.Key, array);
                    }
                    else
                    {
                        model.Set(kvp.Key, kvp.Value);
                    }
                }

                return model;
            }
            else
            {
                return input;
            }
        }

        internal static ValueObject Create(object value)
        {
            switch (value)
            {
                case string temp: return new ValueObject { ValueType = ValueType.String, String = temp };
                case int temp: return new ValueObject { ValueType = ValueType.Int, Int = temp };
                case uint temp: return new ValueObject { ValueType = ValueType.UInt, UInt = temp };
                case long temp: return new ValueObject { ValueType = ValueType.Long, Long = temp };
                case float temp: return new ValueObject { ValueType = ValueType.Float, Float = temp };
                case double temp: return new ValueObject { ValueType = ValueType.Double, Double = temp };
                case bool temp: return new ValueObject { ValueType = ValueType.Bool, Bool = temp };
                case byte[] temp: return new ValueObject { ValueType = ValueType.ByteArray, ByteArray = temp };
                case DateTime temp: return new ValueObject { ValueType = ValueType.DateTime, DateTime = temp.ToUnixTime() };

                default: throw new ArgumentException($"The type { value.GetType().FullName } is not supported.", nameof(value));
            }
        }

        internal static object Value(this ArrayProperty property) => Value(property.Value);
        internal static object Value(this ObjectProperty property) => Value(property.Value);
        internal static object Value(ValueObject value)
        {
            switch (value.ValueType)
            {
                case ValueType.String: return value.String;
                case ValueType.Int: return value.Int;
                case ValueType.UInt: return value.UInt;
                case ValueType.Long: return value.Long;
                case ValueType.Bool: return value.Bool;
                case ValueType.Float: return value.Float;
                case ValueType.Double: return value.Double;
                case ValueType.ByteArray: return value.ByteArray;
                case ValueType.DateTime: return new DateTime(1970, 1, 1).AddMilliseconds(value.DateTime);
                case ValueType.Array: return value.ArrayProperties;
                case ValueType.Object: return value.ObjectProperties;

                default: return null;
            }
        }
    }
}
