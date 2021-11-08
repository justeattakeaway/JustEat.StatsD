using JustEat.StatsD.TagsFormatters;

namespace JustEat.StatsD;

/// <summary>
/// A class representing the configuration options for StatsD usage.
/// </summary>
public class StatsDConfiguration
{
    /// <summary>
    /// The default StatsD port, 8125.
    /// </summary>
    public const int DefaultPort = 8125;

    /// <summary>
    /// Gets the default DNS lookup interval, which is 5 minutes.
    /// </summary>
    public static TimeSpan DefaultDnsLookupInterval => TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the host name or IP address of the StatsD server.
    /// </summary>
    /// <remarks>
    /// A value must be provided for this property.
    /// </remarks>
    public string? Host { get; set; }

    /// <summary>
    /// Gets or sets the port on the StatsD server to use.
    /// </summary>
    public int Port { get; set; } = DefaultPort;

    /// <summary>
    /// Gets or sets the length of time to cache the hostname for IP address lookup using DNS.
    /// </summary>
    /// <remarks>
    /// This value is only used when <see cref="Host"/> is a hostname, rather than an IP address.
    /// <para />
    /// The default value of this property is the value of <see cref="DefaultDnsLookupInterval"/>.
    /// </remarks>
    public TimeSpan? DnsLookupInterval { get; set; } = DefaultDnsLookupInterval;

    /// <summary>
    /// Gets or sets the socket protocol to use, such as using either UDP or IP
    /// sockets to transport stats. The default value is <see cref="JustEat.StatsD.SocketProtocol.Udp"/>.
    /// </summary>
    public SocketProtocol SocketProtocol { get; set; } = SocketProtocol.Udp;

    /// <summary>
    /// Gets or sets an optional prefix to use for all stats.
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatter for tags. By default an instance of <see cref="NoOpTagsFormatter"/> is used.
    /// </summary>
    public IStatsDTagsFormatter TagsFormatter { get; set; } = new NoOpTagsFormatter();

    /// <summary>
    /// Gets or sets an optional delegate to invoke when an error occurs
    /// when sending a metric to the StatsD server.
    /// </summary>
    /// <remarks>
    /// This delegate should return <see langword="true"/> if the exception
    /// was handled and no further action is needed, otherwise <see langword="false"/>
    /// if the exception should be thrown.
    /// <para/>
    /// The default behaviour is to ignore the exception.
    /// </remarks>
    public Func<Exception, bool>? OnError { get; set; }
}
