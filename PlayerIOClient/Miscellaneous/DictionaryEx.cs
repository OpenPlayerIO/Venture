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


        internal static DateTime FromUnixTime(this long input) => new DateTime(1970, 1, 1).AddMilliseconds((long)input);
        internal static long ToUnixTime(this DateTime input) => (long)((input - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
    }
}
