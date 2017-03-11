using System.Globalization;

namespace JustEat.StatsD
{
    public class StatsDConfiguration
    {
        public const int DefaultPort = 8125;
        private static readonly CultureInfo SafeDefaultCulture = new CultureInfo(StatsDMessageFormatter.SafeDefaultIsoCultureId);

        public int Port { get; set; } = DefaultPort;

        public string HostNameOrAddress { get; set; }

        public string Prefix { get; set; } = string.Empty;
        public CultureInfo Culture { get; set; } = SafeDefaultCulture;
    }
}