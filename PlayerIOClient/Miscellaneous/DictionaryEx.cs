using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayerIOClient
{
    internal static class DictionaryEx
    {
        internal static KeyValuePair[] Convert(Dictionary<string, string> dict)
        {
            var keyValuePairs = new List<KeyValuePair>();

            if (dict != null)
                keyValuePairs.AddRange(from kvp in dict select new KeyValuePair { Key = kvp.Key, Value = kvp.Value });

            return keyValuePairs.ToArray();
        }

        internal static Dictionary<string, string> Convert(KeyValuePair[] keyValuePair)
        {
            var dict = new Dictionary<string, string>();

            if (keyValuePair != null)
            {
                foreach (var valuePair in keyValuePair)
                    dict[valuePair.Key] = valuePair.Value;
            }

            return dict;
        }

        internal static Dictionary<string, string> Create(params (string Key, string Value)[] pairs)
        {
            var dictionary = new Dictionary<string, string>();

            foreach (var (Key, Value) in pairs)
            {
                if (Value == null || Key == null)
                    continue;

                dictionary.Add(Key, Value);
            }

            return dictionary;
        }

        internal static Dictionary<string, string> Truncate(this Dictionary<string, string> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                if (kvp.Value == null)
                {
                    dictionary.Remove(kvp.Key);
                }
            }

            return dictionary;
        }
    }
}
