using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerIOClient
{
    internal static class DictionaryEx
    {
        internal static KeyValuePair[] Convert(Dictionary<string, string> dict)
            => (dict ?? new Dictionary<string, string>())
            .Select(x => new KeyValuePair { Key = x.Key, Value = x.Value })
            .ToArray();

        internal static Dictionary<string, string> Convert(KeyValuePair[] keyValuePair)
            => (keyValuePair ?? new KeyValuePair[0]).ToDictionary(x => x.Key, x => x.Value);

        internal static Dictionary<string, string> Create(params (string Key, string Value)[] pairs)
            => pairs
            .Where(x => x.Key != null && x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value);

        internal static Dictionary<string, string> Truncate(this Dictionary<string, string> dictionary)
            => dictionary
            .Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value);
    }

    internal static class ValueObjectEx
    {
        internal static ValueObject Create(this ValueObject obj, object value)
        {
            switch (value)
            {
                case string temp: return new ValueObject(ValueType.String, value);
                case int temp: return new ValueObject(ValueType.Int, value);
                case uint temp: return new ValueObject(ValueType.UInt, value);
                case long temp: return new ValueObject(ValueType.Long, value);
                case float temp: return new ValueObject(ValueType.Float, value);
                case double temp: return new ValueObject(ValueType.Double, value);
                case bool temp: return new ValueObject(ValueType.Bool, value);
                case byte[] temp: return new ValueObject(ValueType.ByteArray, value);

                case DateTime date: return new ValueObject(ValueType.DateTime, date.ToUnixTime());
                case DatabaseObject DatabaseObject: return new ValueObject(ValueType.DatabaseObject, DatabaseObject.Properties.ToArray());
                case DatabaseObject[] DatabaseArray: return new ValueObject(ValueType.DatabaseArray, DatabaseArray.SelectMany(p => p.Properties).ToArray());

                default: throw new ArgumentException($"The type { value.GetType().FullName } is not supported.", nameof(value));
            }
        }
    }
}
