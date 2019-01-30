using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Tson.NET;

namespace PlayerIOClient
{
    /// <summary>
    /// A database object is an object stored in BigDB which has a unique key, and a set of properties.
    /// <para>
    ///     You can set and remove properties, and persist the changes to the database with the Save() method on the root object.
    /// </para>
    /// </summary>
    public partial class DatabaseObject : IDictionary<string, object>
    {
        internal DatabaseObject(BigDB owner, string table, string key, string version, List<ObjectProperty> properties)
        {
            this.Owner = owner;
            this.Table = table;
            this.Key = key;
            this.Version = version;
            this.Properties = (DatabaseEx.FromDictionary(DatabaseEx.ToDictionary(properties)) as DatabaseObject).Properties;
            this.ExistsInDatabase = true;
        }

        /// <summary>
        /// This method allows you to load a Database Object (properties only) from a TSON string.
        /// </summary>
        /// <param name="input"> The TSON string. </param>
        /// <returns> A database object containing the properties of the deserialized TSON. </returns>
        public static DatabaseObject LoadFromString(string input) =>
            DatabaseEx.FromDictionary(TsonConvert.DeserializeObject(input)) as DatabaseObject;

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

        /// <summary>
        /// A boolean representing whether the Database Object has been persisted to BigDB.
        /// </summary>
        internal bool ExistsInDatabase { get; set; }

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
        public void Clear() => this.Properties.Clear();

        /// <summary>
        /// The amount of properties within this object.
        /// </summary>
        public int Count => this.Properties.Count;

        /// <summary>
        /// Return a TSON string of the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => TsonConvert.SerializeObject(this.Properties, Formatting.Indented);

        internal BigDB Owner { get; set; }
    }

    public partial class DatabaseObject
    {
        private const string INVALID_OPEARTION = "The requested method is disabled, please use the public methods instead.";

        [EditorBrowsable(EditorBrowsableState.Never)] public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public bool Contains(KeyValuePair<string, object> item) => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public bool Remove(KeyValuePair<string, object> item) => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public bool TryGetValue(string key, out object value) => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public void Add(KeyValuePair<string, object> item) => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public void Add(string key, object value) => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException(INVALID_OPEARTION);
        [EditorBrowsable(EditorBrowsableState.Never)] public bool IsReadOnly => throw new InvalidOperationException(INVALID_OPEARTION);

        object IDictionary<string, object>.this[string key] { get => this.Properties[key]; set => this.Properties[key] = value; }
    }
}
