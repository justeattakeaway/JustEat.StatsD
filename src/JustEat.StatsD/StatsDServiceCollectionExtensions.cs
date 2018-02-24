#if !NET451
using System;
using JustEat.StatsD.EndpointLookups;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class containing extension methods for registering statsD services with <see cref="IServiceCollection"/>. This class cannot be inherited.
    /// </summary>
    public static class StatsDServiceCollectionExtensions
    {
        /// <summary>
        /// Adds statsD services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to register statsD with.</param>
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
        /// Adds statsD services to the specified <see cref="IServiceCollection"/> for the specified host and optional prefix.
        /// </summary>
        /// <param name="services">The service collection to register statsD with.</param>
        /// <param name="host">The host name or IP address of the statsD server.</param>
        /// <param name="prefix">An optional prefix to prepend to all stats.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> or <paramref name="host"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddStatsD(this IServiceCollection services, string host, string prefix = null)
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
                (configuration) =>
                {
                    configuration.Host = host;
                    configuration.Prefix = prefix ?? string.Empty;
                });
        }

        /// <summary>
        /// Adds statsD services to the specified <see cref="IServiceCollection"/> with the specified configuration.
        /// </summary>
        /// <param name="services">The service collection to register statsD with.</param>
        /// <param name="configuration">The statsD configuration to use.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddStatsD(this IServiceCollection services, StatsDConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.AddStatsDCore();
            services.AddSingleton(configuration);

            return services;
        }

        /// <summary>
        /// Adds StatsD services to the specified <see cref="IServiceCollection"/> with the specified configuration delegate.
        /// </summary>
        /// <param name="services">The service collection to register StatsD with.</param>
        /// <param name="configure">A delegate to a method to use to configure the StatsD configuration.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddStatsD(this IServiceCollection services, Action<StatsDConfiguration> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return services.AddStatsD((_, configuration) => configure(configuration));
        }

        /// <summary>
        /// Adds statsD services to the specified <see cref="IServiceCollection"/> with the specified configuration delegate.
        /// </summary>
        /// <param name="services">The service collection to register statsD with.</param>
        /// <param name="configure">A delegate to a method to use to configure the statsD configuration.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> or <paramref name="configure"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddStatsD(this IServiceCollection services, Action<IServiceProvider, StatsDConfiguration> configure)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            return services.AddStatsD(
                (provider) =>
                {
                    var configuration = new StatsDConfiguration();

                    configure(provider, configuration);

                    return configuration;
                });
        }

        /// <summary>
        /// Adds statsD services to the specified <see cref="IServiceCollection"/> using the specified delegate to create the configuration.
        /// </summary>
        /// <param name="services">The service collection to register statsD with.</param>
        /// <param name="configurationFactory">A delegate to a method to use to create the statsD configuration.</param>
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
            services.TryAddSingleton(ResolveStatsDTransport);
            services.TryAddSingleton(ResolveStatsDPublisher);

            return services;
        }

        private static IStatsDPublisher ResolveStatsDPublisher(IServiceProvider provider)
        {
            var config = provider.GetRequiredService<StatsDConfiguration>();
            var transport = provider.GetRequiredService<IStatsDTransport>();

            return new StatsDPublisher(config, transport);
        }

        private static IStatsDTransport ResolveStatsDTransport(IServiceProvider provider)
        {
            var config = provider.GetRequiredService<StatsDConfiguration>();

            var endpointSource = EndpointParser.MakeEndPointSource(
                config.Host,
                config.Port,
                config.DnsLookupInterval);

            return new UdpTransport(endpointSource);
        }
    }
}
#endif
