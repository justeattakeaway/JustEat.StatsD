using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;

namespace JustEat.StatsD.Diagnostics;

public static class WhenUsingTheMetricsHost
{
    [Fact]
    public static async Task Measurements_Flow_To_StatsD_Through_The_Metrics_Pipeline()
    {
        // Arrange
        var publisher = new FakeStatsDPublisher();

        var builder = Host.CreateEmptyApplicationBuilder(null);
        builder.Services.AddSingleton<IStatsDPublisherWithTags>(publisher);
        builder.Services.AddMetrics((metrics) =>
            metrics.AddStatsD((options) => options.ObservableInstrumentsPollingInterval = TimeSpan.FromMilliseconds(50))
                   .EnableMetrics("HostMeter"));

        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);

        try
        {
            var meterFactory = host.Services.GetRequiredService<IMeterFactory>();
            using var meter = meterFactory.Create("HostMeter");

            // Act - synchronous instruments are delivered as measurements are recorded
            var counter = meter.CreateCounter<int>("requests");
            counter.Add(3, new KeyValuePair<string, object?>("status", 200));

            var histogram = meter.CreateHistogram<double>("request.duration", unit: "s");
            histogram.Record(0.25);

            // Assert
            publisher.Metrics.ShouldContain((metric) => metric.Bucket == "HostMeter.requests");

            var counterMetric = publisher.Metrics.First((metric) => metric.Bucket == "HostMeter.requests");
            counterMetric.Kind.ShouldBe("counter");
            counterMetric.Value.ShouldBe(3);
            counterMetric.Tags.ShouldNotBeNull();
            counterMetric.Tags.ShouldContainKeyAndValue("status", "200");

            publisher.Metrics.ShouldContain((metric) => metric.Bucket == "HostMeter.request.duration");

            var timingMetric = publisher.Metrics.First((metric) => metric.Bucket == "HostMeter.request.duration");
            timingMetric.Kind.ShouldBe("timing");
            timingMetric.Value.ShouldBe(250);

            // Act - observable instruments are polled on a timer
            long bytes = 0;
            _ = meter.CreateObservableCounter("bytes", () => Interlocked.Add(ref bytes, 100));

            // Assert
            await WaitUntilAsync(() => publisher.Metrics.Any((metric) => metric.Bucket == "HostMeter.bytes"));

            var observableMetric = publisher.Metrics.First((metric) => metric.Bucket == "HostMeter.bytes");
            observableMetric.Kind.ShouldBe("counter");
            observableMetric.Value.ShouldBe(100);
        }
        finally
        {
            await host.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    [Fact]
    public static async Task Measurements_Are_Not_Published_For_Meters_That_Are_Not_Enabled()
    {
        // Arrange
        var publisher = new FakeStatsDPublisher();

        var builder = Host.CreateEmptyApplicationBuilder(null);
        builder.Services.AddSingleton<IStatsDPublisherWithTags>(publisher);
        builder.Services.AddMetrics((metrics) => metrics.AddStatsD().EnableMetrics("HostMeter.Enabled"));

        using var host = builder.Build();
        await host.StartAsync(TestContext.Current.CancellationToken);

        try
        {
            var meterFactory = host.Services.GetRequiredService<IMeterFactory>();
            using var meter = meterFactory.Create("HostMeter.Disabled");

            // Act
            var counter = meter.CreateCounter<int>("requests");
            counter.Add(1);

            // Assert
            publisher.Metrics.ShouldBeEmpty();
        }
        finally
        {
            await host.StopAsync(TestContext.Current.CancellationToken);
        }
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var timeout = TimeSpan.FromSeconds(30);
        var delay = TimeSpan.FromMilliseconds(50);
        var stopAt = DateTimeOffset.UtcNow.Add(timeout);

        while (!condition() && DateTimeOffset.UtcNow < stopAt)
        {
            await Task.Delay(delay, TestContext.Current.CancellationToken);
        }

        condition().ShouldBeTrue("The condition was not satisfied within the timeout.");
    }
}
