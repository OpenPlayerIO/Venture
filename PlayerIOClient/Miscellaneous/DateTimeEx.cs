using System;

namespace PlayerIOClient
{
    internal static class DateTimeEx
    {
        internal static DateTime FromUnixTime(this long input) => new DateTime(1970, 1, 1).AddMilliseconds((long)input);
        internal static long ToUnixTime(this DateTime input) => (long)((input - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
    }
}
