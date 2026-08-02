using JustEat.StatsD.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace JustEat.StatsD;

/// <summary>
/// A class containing extension methods for registering StatsD with <see cref="IMetricsBuilder"/>. This class cannot be inherited.
/// </summary>
public static class StatsDMetricsBuilderExtensions
{
    /// <summary>
    /// Adds a listener to the specified <see cref="IMetricsBuilder"/> that publishes
    /// measurements from enabled instruments to StatsD.
    /// </summary>
    /// <param name="builder">The <see cref="IMetricsBuilder"/> to add the StatsD listener to.</param>
    /// <returns>
    /// The value specified by <paramref name="builder"/>.
    /// </returns>
    /// <remarks>
    /// An implementation of <see cref="IStatsDPublisherWithTags"/> must be registered with
    /// the service collection, such as by using <see cref="StatsDServiceCollectionExtensions"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static IMetricsBuilder AddStatsD(this IMetricsBuilder builder)
        => AddStatsD(builder, configureMetrics: null);

    /// <summary>
    /// Adds a listener to the specified <see cref="IMetricsBuilder"/> that publishes
    /// measurements from enabled instruments to StatsD.
    /// </summary>
    /// <param name="builder">The <see cref="IMetricsBuilder"/> to add the StatsD listener to.</param>
    /// <param name="configureMetrics">An optional delegate to use to configure the <see cref="StatsDMetricsOptions"/> to use.</param>
    /// <returns>
    /// The value specified by <paramref name="builder"/>.
    /// </returns>
    /// <remarks>
    /// An implementation of <see cref="IStatsDPublisherWithTags"/> must be registered with
    /// the service collection, such as by using <see cref="StatsDServiceCollectionExtensions"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="builder"/> is <see langword="null"/>.
    /// </exception>
    public static IMetricsBuilder AddStatsD(this IMetricsBuilder builder, Action<StatsDMetricsOptions>? configureMetrics)
    {
#if NET
        ArgumentNullException.ThrowIfNull(builder);
#else
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
#endif

        builder.Services.AddOptions();

        if (configureMetrics != null)
        {
            builder.Services.Configure(configureMetrics);
        }

        return builder.AddListener<StatsDMetricsListener>();
    }
}
