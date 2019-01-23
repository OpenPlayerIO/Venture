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

        protected PlayerIOError WrongType(string attemptedType, ValueType actualType)
        {
            return new PlayerIOError(ErrorCode.GeneralError, string.Concat(new object[]
            {
                "The value is not ",
                attemptedType,
                ", it's type is: ",
                actualType
            }));
        }

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
