using System;
using System.Globalization;

namespace JustEat.StatsD
{
    public class StatsDConfiguration
    {
        public const int DefaultPort = 8125;
        public static readonly TimeSpan DefaultDnsLookupInterval = TimeSpan.FromMinutes(5);

        /// <summary>
        /// The host name or IP address of the statsD server. 
        /// This field must be set. 
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The port on the satsD server that stas are sent to.
        /// Default is the standard port (8125).
        /// </summary>
        public int Port { get; set; } = DefaultPort;

        /// <summary>
        /// Length of time to cache the host name to Ip address lookup.
        /// Only used when "Host" contains a host name.
        /// Default is 5 minutes.
        /// </summary>
        public TimeSpan? DnsLookupInterval { get; set; } = DefaultDnsLookupInterval;

        /// <summary>
        /// Prepend a prefix to all stats.
        /// Default is empty.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Culture for formatting stats.
        /// Default is InvariantCulture.
        /// </summary>
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    }
}