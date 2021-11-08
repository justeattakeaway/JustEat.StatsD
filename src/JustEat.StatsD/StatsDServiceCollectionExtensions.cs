using JustEat.StatsD.EndpointLookups;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustEat.StatsD;

/// <summary>
/// A class containing extension methods for registering StatsD services with <see cref="IServiceCollection"/>. This class cannot be inherited.
/// </summary>
public static class StatsDServiceCollectionExtensions
{
    /// <summary>
    /// Adds StatsD services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register StatsD with.</param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddStatsD(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddStatsDCore();
        services.TryAddSingleton<StatsDConfiguration>();

        return services;
    }

    /// <summary>
    /// Adds StatsD services to the specified <see cref="IServiceCollection"/> for the specified host and optional prefix.
    /// </summary>
    /// <param name="services">The service collection to register StatsD with.</param>
    /// <param name="host">The host name or IP address of the StatsD server.</param>
    /// <param name="prefix">An optional prefix to prepend to all stats.</param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="host"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddStatsD(this IServiceCollection services, string host, string? prefix = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        return services.AddStatsD(
            (_) =>
            {
                return new StatsDConfiguration()
                {
                    Host = host,
                    Prefix = prefix ?? string.Empty,
                };
            });
    }

    /// <summary>
    /// Adds StatsD services to the specified <see cref="IServiceCollection"/> using the specified delegate to create the configuration.
    /// </summary>
    /// <param name="services">The service collection to register StatsD with.</param>
    /// <param name="configurationFactory">A delegate to a method to use to create the StatsD configuration.</param>
    /// <returns>
    /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="services"/> or <paramref name="configurationFactory"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddStatsD(this IServiceCollection services, Func<IServiceProvider, StatsDConfiguration> configurationFactory)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configurationFactory == null)
        {
            throw new ArgumentNullException(nameof(configurationFactory));
        }

        services.AddStatsDCore();
        services.AddSingleton(configurationFactory);

        return services;
    }

    private static IServiceCollection AddStatsDCore(this IServiceCollection services)
    {
        services.TryAddSingleton(ResolveEndPointSource);
        services.TryAddSingleton(ResolveStatsDTransport);
        services.TryAddSingleton(ResolveStatsDPublisher);

        return services;
    }

    private static IEndPointSource ResolveEndPointSource(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<StatsDConfiguration>();

        return EndPointFactory.MakeEndPointSource(
            config.Host!,
            config.Port,
            config.DnsLookupInterval);
    }

    private static IStatsDPublisher ResolveStatsDPublisher(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<StatsDConfiguration>();
        var socketProtocol = provider.GetRequiredService<IStatsDTransport>();

        return new StatsDPublisher(config, socketProtocol);
    }

    private static IStatsDTransport ResolveStatsDTransport(IServiceProvider provider)
    {
        var endpointSource = provider.GetRequiredService<IEndPointSource>();
        return new SocketTransport(endpointSource, SocketProtocol.Udp);
    }
}
