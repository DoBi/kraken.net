using System;

namespace Kraken.Net
{
    /// <summary>
    /// A timestamp helper class
    /// </summary>
    internal static class Timestamp
    {
        private static DateTime unixOrigin = new DateTime(1970, 1, 1);

        /// <summary>
        /// Get a unix timestamp from milliseconds
        /// </summary>
        public static Int64 Microtime(this DateTime datetime)
        {
            TimeSpan diff = datetime - unixOrigin;
            return (long) diff.TotalMilliseconds;
        }
    }
}