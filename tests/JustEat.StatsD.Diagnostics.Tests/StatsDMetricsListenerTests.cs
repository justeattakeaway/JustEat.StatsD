using System.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace JustEat.StatsD.Diagnostics;

public static class StatsDMetricsListenerTests
{
    [Fact]
    public static void Constructor_Validates_Arguments()
    {
        // Arrange
        var publisher = new FakeStatsDPublisher();
        var options = Options.Create(new StatsDMetricsOptions());

        // Act and Assert
        Should.Throw<ArgumentNullException>(() => new StatsDMetricsListener(null!, options)).ParamName.ShouldBe("publisher");
        Should.Throw<ArgumentNullException>(() => new StatsDMetricsListener(publisher, null!)).ParamName.ShouldBe("options");
    }

    [Fact]
    public static void Listener_Has_Expected_Name()
    {
        // Arrange
        using var listener = CreateListener(out _);

        // Assert
        listener.Name.ShouldBe("JustEat.StatsD");
        StatsDMetricsListener.ListenerName.ShouldBe("JustEat.StatsD");
    }

    [Fact]
    public static void Counter_Measurements_Are_Published_As_Counters()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.Counters");
        var counter = meter.CreateCounter<int>("requests");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 5, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("counter");
        metric.Value.ShouldBe(5);
        metric.SampleRate.ShouldBe(1);
        metric.Bucket.ShouldBe("TestMeter.Counters.requests");
        metric.Tags.ShouldBeNull();
    }

    [Fact]
    public static void UpDownCounter_Measurements_Are_Published_As_Counters()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.UpDown");
        var counter = meter.CreateUpDownCounter<long>("connections");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().LongHandler!(counter, -2, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("counter");
        metric.Value.ShouldBe(-2);
    }

    [Fact]
    public static void Histogram_Measurements_In_Seconds_Are_Published_As_Timers_In_Milliseconds()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.Histograms");
        var histogram = meter.CreateHistogram<double>("http.server.request.duration", unit: "s");

        // Act
        listener.InstrumentPublished(histogram, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().DoubleHandler!(histogram, 1.234, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("timing");
        metric.Value.ShouldBe(1234);
        metric.Bucket.ShouldBe("TestMeter.Histograms.http.server.request.duration");
    }

    [Fact]
    public static void Integral_Histogram_Measurements_In_Seconds_Are_Published_As_Timers_In_Milliseconds()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.IntegralHistograms");
        var histogram = meter.CreateHistogram<long>("duration", unit: "s");

        // Act
        listener.InstrumentPublished(histogram, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().LongHandler!(histogram, 3, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("timing");
        metric.IntegralValue.ShouldBe(3000);
    }

    [Fact]
    public static void Histogram_Measurements_Not_In_Seconds_Are_Not_Converted()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.HistogramsMs");
        var histogram = meter.CreateHistogram<double>("duration", unit: "ms");

        // Act
        listener.InstrumentPublished(histogram, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().DoubleHandler!(histogram, 42.4, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("timing");
        metric.Value.ShouldBe(42);
    }

    [Fact]
    public static void Histogram_Conversion_Can_Be_Disabled()
    {
        // Arrange
        var options = new StatsDMetricsOptions() { ConvertSecondsToMilliseconds = false };
        using var listener = CreateListener(out var publisher, options);
        using var meter = new Meter("TestMeter.HistogramsRaw");
        var histogram = meter.CreateHistogram<double>("duration", unit: "s");

        // Act
        listener.InstrumentPublished(histogram, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().DoubleHandler!(histogram, 1.6, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("timing");
        metric.Value.ShouldBe(2);
    }

    [Fact]
    public static void Gauge_Measurements_Are_Published_As_Gauges()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.Gauges");
        var gauge = meter.CreateGauge<double>("temperature");

        // Act
        listener.InstrumentPublished(gauge, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().DoubleHandler!(gauge, 21.5, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("gauge");
        metric.Value.ShouldBe(21.5);
    }

    [Fact]
    public static void ObservableGauge_Measurements_Are_Published_As_Gauges()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.ObservableGauges");
        var gauge = meter.CreateObservableGauge("cpu", () => 0.5);

        // Act
        listener.InstrumentPublished(gauge, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().DoubleHandler!(gauge, 0.5, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("gauge");
        metric.Value.ShouldBe(0.5);
    }

    [Fact]
    public static void ObservableUpDownCounter_Measurements_Are_Published_As_Gauges()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.ObservableUpDown");
        var counter = meter.CreateObservableUpDownCounter("queue.length", () => 8L);

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().LongHandler!(counter, 8, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("gauge");
        metric.Value.ShouldBe(8);
    }

    [Fact]
    public static void ObservableCounter_Measurements_Are_Published_As_Counter_Deltas()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.ObservableCounters");
        var counter = meter.CreateObservableCounter("bytes", () => 0L);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().LongHandler!;

        // Act and Assert - the first observation is published in full
        handler(counter, 10, default, state);
        publisher.Metrics.Count.ShouldBe(1);
        publisher.Metrics.Last().Value.ShouldBe(10);
        publisher.Metrics.Last().Kind.ShouldBe("counter");

        // Subsequent observations publish the delta
        handler(counter, 25, default, state);
        publisher.Metrics.Count.ShouldBe(2);
        publisher.Metrics.Last().Value.ShouldBe(15);

        // An unchanged total publishes nothing
        handler(counter, 25, default, state);
        publisher.Metrics.Count.ShouldBe(2);

        // A decreasing total is treated as a counter reset
        handler(counter, 5, default, state);
        publisher.Metrics.Count.ShouldBe(3);
        publisher.Metrics.Last().Value.ShouldBe(5);
    }

    [Fact]
    public static void ObservableCounter_Deltas_Are_Tracked_Per_Tag_Set()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.TaggedObservableCounters");
        var counter = meter.CreateObservableCounter("bytes", () => 0L);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().LongHandler!;

        var sent = new[] { new KeyValuePair<string, object?>("direction", "sent") };
        var received = new[] { new KeyValuePair<string, object?>("direction", "received") };

        // Act
        handler(counter, 10, sent, state);
        handler(counter, 100, received, state);
        handler(counter, 15, sent, state);
        handler(counter, 150, received, state);

        // Assert
        var metrics = publisher.Metrics.ToArray();
        metrics.Length.ShouldBe(4);
        metrics[0].Value.ShouldBe(10);
        metrics[1].Value.ShouldBe(100);
        metrics[2].Value.ShouldBe(5);
        metrics[3].Value.ShouldBe(50);
    }

    [Fact]
    public static void ObservableCounter_Deltas_Are_Exact_For_Totals_Larger_Than_A_Double_Can_Represent()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.LargeObservableCounters");
        var counter = meter.CreateObservableCounter("bytes", () => 0L);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().LongHandler!;

        // 2^53 is the largest integer for which every smaller integer is exactly
        // representable as a double, so deltas either side of it would otherwise
        // be quantized or lost entirely.
        const long Threshold = 1L << 53;

        // Act
        handler(counter, Threshold, default, state);
        handler(counter, Threshold + 1, default, state);
        handler(counter, Threshold + 4, default, state);

        // Assert
        var metrics = publisher.Metrics.ToArray();
        metrics.Length.ShouldBe(3);
        metrics[0].IntegralValue.ShouldBe(Threshold);
        metrics[1].IntegralValue.ShouldBe(1);
        metrics[2].IntegralValue.ShouldBe(3);
    }

    [Fact]
    public static void Integral_Measurements_Are_Published_Without_Loss_Of_Precision()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.LargeIntegrals");
        var counter = meter.CreateCounter<long>("bytes");
        var histogram = meter.CreateHistogram<long>("size", unit: "By");

        const long Value = long.MaxValue - 1;

        // Act
        listener.InstrumentPublished(counter, out object? counterState).ShouldBeTrue();
        listener.InstrumentPublished(histogram, out object? histogramState).ShouldBeTrue();

        var handler = listener.GetMeasurementHandlers().LongHandler!;

        handler(counter, Value, default, counterState);
        handler(histogram, Value, default, histogramState);

        // Assert
        var metrics = publisher.Metrics.ToArray();
        metrics.Length.ShouldBe(2);
        metrics[0].Kind.ShouldBe("counter");
        metrics[0].IntegralValue.ShouldBe(Value);
        metrics[1].Kind.ShouldBe("timing");
        metrics[1].IntegralValue.ShouldBe(Value);
    }

    [Fact]
    public static void Fractional_Counter_Measurements_Accumulate_Instead_Of_Being_Lost()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.FractionalCounters");
        var counter = meter.CreateCounter<double>("requests");

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().DoubleHandler!;

        // Act and Assert - each delta is below the rounding threshold, so nothing is published individually...
        handler(counter, 0.4, default, state);
        handler(counter, 0.4, default, state);
        publisher.Metrics.ShouldBeEmpty();

        // ...but the accumulated fractional parts are published once they exceed a whole unit.
        handler(counter, 0.4, default, state);
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Kind.ShouldBe("counter");
        metric.Value.ShouldBe(1);

        // Over many measurements the published total matches the recorded total without drift.
        handler(counter, 0.4, default, state);
        handler(counter, 0.4, default, state);
        publisher.Metrics.Sum((measurement) => measurement.Value).ShouldBe(2);
    }

    [Fact]
    public static void Instrument_Tags_Are_Published_With_Measurement_Tags_Taking_Precedence()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.InstrumentTags");
        var counter = meter.CreateCounter<int>(
            "requests",
            unit: null,
            description: null,
            tags: [new("host", "server1"), new("region", "eu")]);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();

        var tags = new[]
        {
            new KeyValuePair<string, object?>("status", 200),
            new KeyValuePair<string, object?>("region", "us"),
        };

        // Act
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, tags, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Tags.ShouldNotBeNull();
        metric.Tags.ShouldBe(new Dictionary<string, string?>()
        {
            ["host"] = "server1",
            ["region"] = "us",
            ["status"] = "200",
        });
    }

    [Fact]
    public static void Instrument_Tags_Are_Published_When_There_Are_No_Measurement_Tags()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.InstrumentOnlyTags");
        var counter = meter.CreateCounter<int>(
            "requests",
            unit: null,
            description: null,
            tags: [new("host", "server1")]);

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Tags.ShouldNotBeNull();
        metric.Tags.ShouldBe(new Dictionary<string, string?>() { ["host"] = "server1" });
    }

    [Fact]
    public static void Tags_Are_Converted_To_Strings()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.Tags");
        var counter = meter.CreateCounter<int>("requests");

        var tags = new[]
        {
            new KeyValuePair<string, object?>("string", "value"),
            new KeyValuePair<string, object?>("int", 200),
            new KeyValuePair<string, object?>("bool", true),
            new KeyValuePair<string, object?>("null", null),
            new KeyValuePair<string, object?>("double", 1.5),
        };

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, tags, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Tags.ShouldNotBeNull();
        metric.Tags.ShouldBe(new Dictionary<string, string?>()
        {
            ["string"] = "value",
            ["int"] = "200",
            ["bool"] = "true",
            ["null"] = null,
            ["double"] = "1.5",
        });
    }

    [Fact]
    public static void Bucket_Names_Are_Sanitized()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("My Meter:v1");
        var counter = meter.CreateCounter<int>("requests|total@host");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Bucket.ShouldBe("My_Meter_v1.requests_total_host");
    }

    [Fact]
    public static void Bucket_Names_Can_Be_Customized()
    {
        // Arrange
        var options = new StatsDMetricsOptions()
        {
            BucketNameProvider = (instrument) => "custom." + instrument.Name,
        };

        using var listener = CreateListener(out var publisher, options);
        using var meter = new Meter("TestMeter.CustomBuckets");
        var counter = meter.CreateCounter<int>("requests");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Bucket.ShouldBe("custom.requests");
    }

    [Fact]
    public static void Custom_Bucket_Names_Are_Sanitized()
    {
        // Arrange
        var options = new StatsDMetricsOptions()
        {
            BucketNameProvider = (instrument) => "custom bucket:" + instrument.Name + "|total@host",
        };

        using var listener = CreateListener(out var publisher, options);
        using var meter = new Meter("TestMeter.CustomSanitizedBuckets");
        var counter = meter.CreateCounter<int>("requests");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Bucket.ShouldBe("custom_bucket_requests_total_host");
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(0)]
    [InlineData(-0.5)]
    [InlineData(1.5)]
    public static void Invalid_Sample_Rates_Are_Ignored(double sampleRate)
    {
        // Arrange
        var options = new StatsDMetricsOptions()
        {
            SampleRateProvider = (_) => sampleRate,
        };

        using var listener = CreateListener(out var publisher, options);
        using var meter = new Meter("TestMeter.InvalidSampleRates");
        var counter = meter.CreateCounter<int>("requests");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.SampleRate.ShouldBe(1);
    }

    [Fact]
    public static void Counter_Residuals_Are_Tracked_Per_Tag_Set_Regardless_Of_Tag_Order()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.ReorderedCounterTags");
        var counter = meter.CreateCounter<double>("requests");

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().DoubleHandler!;

        var tags = new[]
        {
            new KeyValuePair<string, object?>("host", "server1"),
            new KeyValuePair<string, object?>("region", "eu"),
        };

        var reordered = new[] { tags[1], tags[0] };

        // Act - the same tags recorded in a different order are the same time series
        handler(counter, 0.5, tags, state);
        handler(counter, 0.5, reordered, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.Value.ShouldBe(1);
    }

    [Fact]
    public static void ObservableCounter_Deltas_Are_Tracked_Per_Tag_Set_Regardless_Of_Tag_Order()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.ReorderedObservableCounterTags");
        var counter = meter.CreateObservableCounter("bytes", () => 0L);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().LongHandler!;

        var tags = new[]
        {
            new KeyValuePair<string, object?>("host", "server1"),
            new KeyValuePair<string, object?>("region", "eu"),
        };

        var reordered = new[] { tags[1], tags[0] };

        // Act
        handler(counter, 10, tags, state);
        handler(counter, 25, reordered, state);

        // Assert - the second observation is a delta, not a new time series
        var metrics = publisher.Metrics.ToArray();
        metrics.Length.ShouldBe(2);
        metrics[0].Value.ShouldBe(10);
        metrics[1].Value.ShouldBe(15);
    }

    [Fact]
    public static void Null_And_Empty_Tag_Values_Are_Tracked_Separately()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.NullAndEmptyTags");
        var counter = meter.CreateObservableCounter("bytes", () => 0L);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().LongHandler!;

        var nullValue = new[] { new KeyValuePair<string, object?>("region", null) };
        var emptyValue = new[] { new KeyValuePair<string, object?>("region", string.Empty) };

        // Act
        handler(counter, 10, nullValue, state);
        handler(counter, 20, emptyValue, state);

        // Assert - the two tag sets are different time series, so both totals are published in full
        var metrics = publisher.Metrics.ToArray();
        metrics.Length.ShouldBe(2);
        metrics[0].Value.ShouldBe(10);
        metrics[1].Value.ShouldBe(20);
    }

    [Fact]
    public static void Tag_Sets_Larger_Than_The_Stack_Allocation_Threshold_Are_Tracked_Correctly()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.ManyTags");
        var counter = meter.CreateObservableCounter("bytes", () => 0L);

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handler = listener.GetMeasurementHandlers().LongHandler!;

        var tags = Enumerable.Range(0, 20)
            .Select((i) => new KeyValuePair<string, object?>($"tag{i:00}", i))
            .ToArray();

        var reordered = tags.Reverse().ToArray();

        // Act
        handler(counter, 10, tags, state);
        handler(counter, 25, reordered, state);

        // Assert
        var metrics = publisher.Metrics.ToArray();
        metrics.Length.ShouldBe(2);
        metrics[0].Value.ShouldBe(10);
        metrics[1].Value.ShouldBe(15);
    }

    [Fact]
    public static void Sample_Rates_Can_Be_Customized()
    {
        // Arrange
        var options = new StatsDMetricsOptions()
        {
            SampleRateProvider = (_) => 0.5,
        };

        using var listener = CreateListener(out var publisher, options);
        using var meter = new Meter("TestMeter.SampleRates");
        var counter = meter.CreateCounter<int>("requests");

        // Act
        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, state);

        // Assert
        var metric = publisher.Metrics.ShouldHaveSingleItem();
        metric.SampleRate.ShouldBe(0.5);
    }

    [Fact]
    public static void Unknown_Instrument_Types_Are_Not_Enabled()
    {
        // Arrange
        using var listener = CreateListener(out _);
        using var meter = new Meter("TestMeter.Unknown");
        var instrument = new CustomInstrument(meter, "custom");

        // Act and Assert
        listener.InstrumentPublished(instrument, out object? state).ShouldBeFalse();
        state.ShouldBeNull();
    }

    [Fact]
    public static void All_Numeric_Measurement_Types_Are_Published()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.Numeric");
        var counter = meter.CreateCounter<byte>("requests");

        listener.InstrumentPublished(counter, out object? state).ShouldBeTrue();
        var handlers = listener.GetMeasurementHandlers();

        // Act
        handlers.ByteHandler!(counter, 1, default, state);
        handlers.ShortHandler!(counter, 2, default, state);
        handlers.IntHandler!(counter, 3, default, state);
        handlers.LongHandler!(counter, 4, default, state);
        handlers.FloatHandler!(counter, 5.4f, default, state);
        handlers.DoubleHandler!(counter, 6.5, default, state);
        handlers.DecimalHandler!(counter, 7.6m, default, state);

        // Assert
        publisher.Metrics.Select((metric) => metric.Value).ShouldBe([1, 2, 3, 4, 5, 7, 8]);
    }

    [Fact]
    public static void Measurements_With_No_State_Are_Ignored()
    {
        // Arrange
        using var listener = CreateListener(out var publisher);
        using var meter = new Meter("TestMeter.NoState");
        var counter = meter.CreateCounter<int>("requests");

        // Act
        listener.GetMeasurementHandlers().IntHandler!(counter, 1, default, null);

        // Assert
        publisher.Metrics.ShouldBeEmpty();
    }

    [Fact]
    public static void MeasurementsCompleted_Does_Not_Throw()
    {
        // Arrange
        using var listener = CreateListener(out _);
        using var meter = new Meter("TestMeter.Completed");
        var counter = meter.CreateCounter<int>("requests");

        // Act and Assert
        listener.MeasurementsCompleted(counter, null);
    }

    [Fact]
    public static void Dispose_Can_Be_Called_Multiple_Times()
    {
        // Arrange
        var listener = CreateListener(out _);

        // Act and Assert
        listener.Dispose();
        listener.Dispose();
    }

    private static StatsDMetricsListener CreateListener(out FakeStatsDPublisher publisher, StatsDMetricsOptions? options = null)
    {
        publisher = new FakeStatsDPublisher();
        return new StatsDMetricsListener(publisher, Options.Create(options ?? new StatsDMetricsOptions()));
    }

    private sealed class CustomInstrument(Meter meter, string name) : Instrument<int>(meter, name, null, null);
}
