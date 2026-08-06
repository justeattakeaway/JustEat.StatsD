using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace JustEat.StatsD.Diagnostics;

public static class WhenRegisteringStatsDMetrics
{
    [Fact]
    public static void Can_Register_The_StatsD_Listener()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IStatsDPublisherWithTags>(new FakeStatsDPublisher());

        // Act
        services.AddMetrics((metrics) => metrics.AddStatsD());

        // Assert
        using var provider = services.BuildServiceProvider();
        var listener = provider.GetServices<IMetricsListener>().ShouldHaveSingleItem();
        listener.ShouldBeOfType<StatsDMetricsListener>();
    }

    [Fact]
    public static void Registering_The_Listener_Twice_Adds_One_Listener()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IStatsDPublisherWithTags>(new FakeStatsDPublisher());

        // Act
        services.AddMetrics((metrics) => metrics.AddStatsD().AddStatsD());

        // Assert
        using var provider = services.BuildServiceProvider();
        var listener = provider.GetServices<IMetricsListener>().ShouldHaveSingleItem();
        listener.ShouldBeOfType<StatsDMetricsListener>();
    }

    [Fact]
    public static void Can_Configure_The_Options()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IStatsDPublisherWithTags>(new FakeStatsDPublisher());

        // Act
        services.AddMetrics((metrics) => metrics.AddStatsD((options) =>
        {
            options.ConvertSecondsToMilliseconds = false;
            options.ObservableInstrumentsPollingInterval = TimeSpan.FromSeconds(1);
        }));

        // Assert
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<StatsDMetricsOptions>>().Value;
        options.ConvertSecondsToMilliseconds.ShouldBeFalse();
        options.ObservableInstrumentsPollingInterval.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public static void Options_Have_Expected_Defaults()
    {
        // Arrange
        var options = new StatsDMetricsOptions();

        // Assert
        options.ObservableInstrumentsPollingInterval.ShouldBe(TimeSpan.FromSeconds(10));
        options.ConvertSecondsToMilliseconds.ShouldBeTrue();
        options.BucketNameProvider.ShouldBeNull();
        options.SampleRateProvider.ShouldBeNull();
    }

    [Fact]
    public static void AddStatsD_Validates_Arguments()
    {
        // Arrange
        IMetricsBuilder builder = null!;

        // Act and Assert
        Should.Throw<ArgumentNullException>(() => builder.AddStatsD()).ParamName.ShouldBe("builder");
    }
}
