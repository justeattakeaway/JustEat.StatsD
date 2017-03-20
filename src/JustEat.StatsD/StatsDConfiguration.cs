using System;
using System.Globalization;

namespace JustEat.StatsD
{
    public class StatsDConfiguration
    {
        public const int DefaultPort = 8125;
        public static readonly TimeSpan DefaultDnsLookupInterval = TimeSpan.FromMinutes(5);

        public string Host { get; set; }

        public int Port { get; set; } = DefaultPort;

        public TimeSpan? DnsLookupInterval { get; set; } = DefaultDnsLookupInterval;

        public string Prefix { get; set; } = string.Empty;
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;
    }
}