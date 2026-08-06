using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Options;

namespace JustEat.StatsD.Diagnostics;

/// <summary>
/// A class representing an implementation of <see cref="IMetricsListener"/> that publishes
/// measurements recorded by <see cref="System.Diagnostics.Metrics"/> instruments to StatsD.
/// This class cannot be inherited.
/// </summary>
/// <remarks>
/// <para>
/// Instruments are published as follows: <see cref="Counter{T}"/> and <see cref="UpDownCounter{T}"/>
/// measurements are published as StatsD counters; <see cref="Histogram{T}"/> measurements are
/// published as StatsD timers; <see cref="Gauge{T}"/>, <see cref="ObservableGauge{T}"/> and
/// <see cref="ObservableUpDownCounter{T}"/> measurements are published as StatsD gauges; and
/// <see cref="ObservableCounter{T}"/> measurements are converted from cumulative totals to
/// deltas and published as StatsD counters.
/// </para>
/// <para>
/// Observable instruments are polled at the interval specified by
/// <see cref="StatsDMetricsOptions.ObservableInstrumentsPollingInterval"/>.
/// </para>
/// </remarks>
public sealed class StatsDMetricsListener : IMetricsListener, IDisposable
{
    /// <summary>
    /// The name of the listener used to identify it in metrics configuration rules.
    /// </summary>
    public const string ListenerName = "JustEat.StatsD";

    private readonly IStatsDPublisherWithTags _publisher;
    private readonly StatsDMetricsOptions _options;
    private readonly MeasurementHandlers _handlers;
    private readonly object _syncRoot = new();

    private IObservableInstrumentsSource? _source;
    private Timer? _timer;
    private bool _hasObservableInstruments;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsDMetricsListener"/> class.
    /// </summary>
    /// <param name="publisher">The <see cref="IStatsDPublisherWithTags"/> to publish measurements with.</param>
    /// <param name="options">The <see cref="StatsDMetricsOptions"/> to use.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="publisher"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public StatsDMetricsListener(IStatsDPublisherWithTags publisher, IOptions<StatsDMetricsOptions> options)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        // Integral measurements are kept on a long code path so that values that cannot be
        // represented exactly by a double (such as the cumulative byte counters reported by
        // the built-in .NET meters in a long-running process) are published without loss.
        _handlers = new MeasurementHandlers()
        {
            ByteHandler = (instrument, value, tags, state) => Record(instrument, value, tags, state),
            ShortHandler = (instrument, value, tags, state) => Record(instrument, value, tags, state),
            IntHandler = (instrument, value, tags, state) => Record(instrument, value, tags, state),
            LongHandler = (instrument, value, tags, state) => Record(instrument, value, tags, state),
            FloatHandler = (instrument, value, tags, state) => Record(instrument, (double)value, tags, state),
            DoubleHandler = (instrument, value, tags, state) => Record(instrument, value, tags, state),
            DecimalHandler = (instrument, value, tags, state) => Record(instrument, (double)value, tags, state),
        };
    }

    /// <inheritdoc />
    public string Name => ListenerName;

    /// <inheritdoc />
    public MeasurementHandlers GetMeasurementHandlers() => _handlers;

    /// <inheritdoc />
    public bool InstrumentPublished(Instrument instrument, out object? userState)
    {
        userState = InstrumentState.TryCreate(instrument, _options);

        if (userState is null)
        {
            return false;
        }

        if (instrument.IsObservable)
        {
            lock (_syncRoot)
            {
                _hasObservableInstruments = true;
                EnsureTimer();
            }
        }

        return true;
    }

    /// <inheritdoc />
    public void MeasurementsCompleted(Instrument instrument, object? userState)
    {
        // There is no per-instrument state to release beyond the user state itself.
    }

    /// <inheritdoc />
    public void Initialize(IObservableInstrumentsSource source)
    {
        lock (_syncRoot)
        {
            _source = source;
            EnsureTimer();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_syncRoot)
        {
            _disposed = true;
            _timer?.Dispose();
            _timer = null;
        }
    }

    private void EnsureTimer()
    {
        if (_disposed || _timer is not null || _source is null || !_hasObservableInstruments)
        {
            return;
        }

        var interval = _options.ObservableInstrumentsPollingInterval;

        if (interval <= TimeSpan.Zero)
        {
            interval = TimeSpan.FromSeconds(10);
        }

        _timer = new Timer(static (state) => ((StatsDMetricsListener)state!).PollObservableInstruments(), this, interval, interval);
    }

#pragma warning disable CA1031
    private void PollObservableInstruments()
    {
        try
        {
            _source?.RecordObservableInstruments();
        }
        catch (Exception)
        {
            // Swallow exceptions thrown by observable instrument callbacks so
            // that they do not terminate the process from the timer thread.
        }
    }
#pragma warning restore CA1031

    private void Record(Instrument instrument, long value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        if (state is not InstrumentState instrumentState)
        {
            return;
        }

        var statsDTags = CreateTags(instrumentState, tags);

        switch (instrumentState.Kind)
        {
            case StatsDMetricKind.Counter:
                // An integral counter reports whole deltas, so there is no residual to accumulate.
                _publisher.Increment(value, instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                break;

            case StatsDMetricKind.CumulativeCounter:
                long delta = instrumentState.NextDelta(InstrumentState.TagsKey(tags), value);

                if (delta != 0)
                {
                    _publisher.Increment(delta, instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                }

                break;

            case StatsDMetricKind.Gauge:
                // IStatsDPublisherWithTags.Gauge() accepts a double, so an integral
                // measurement cannot be published any more precisely than this.
                _publisher.Gauge(value, instrumentState.Bucket, statsDTags);
                break;

            case StatsDMetricKind.Timing:
                _publisher.Timing(Scale(value, instrumentState.ScaleFactor), instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                break;

            default:
                break;
        }
    }

    private void Record(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
    {
        if (state is not InstrumentState instrumentState)
        {
            return;
        }

        var statsDTags = CreateTags(instrumentState, tags);

        switch (instrumentState.Kind)
        {
            case StatsDMetricKind.Counter:
                if (instrumentState.AccumulatesResidual)
                {
                    long increment = instrumentState.NextCounterDelta(InstrumentState.TagsKey(tags), value);

                    if (increment != 0)
                    {
                        _publisher.Increment(increment, instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                    }
                }
                else
                {
                    _publisher.Increment(RoundToLong(value), instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                }

                break;

            case StatsDMetricKind.CumulativeCounter:
                long delta = instrumentState.NextDelta(InstrumentState.TagsKey(tags), value);

                if (delta != 0)
                {
                    _publisher.Increment(delta, instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                }

                break;

            case StatsDMetricKind.Gauge:
                _publisher.Gauge(value, instrumentState.Bucket, statsDTags);
                break;

            case StatsDMetricKind.Timing:
                _publisher.Timing(RoundToLong(value * instrumentState.ScaleFactor), instrumentState.SampleRate, instrumentState.Bucket, statsDTags);
                break;

            default:
                break;
        }
    }

    private static long RoundToLong(double value)
        => (long)Math.Round(value, MidpointRounding.AwayFromZero);

    // An unscaled integral timing is published as-is so that it does not lose
    // precision by being converted to a double just to be multiplied by one.
    private static long Scale(long value, double scaleFactor)
        => scaleFactor == 1 ? value : RoundToLong(value * scaleFactor);

    private static Dictionary<string, string?>? CreateTags(InstrumentState state, ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        var instrumentTags = state.InstrumentTags;

        if (tags.IsEmpty && instrumentTags is null)
        {
            return null;
        }

        var result = new Dictionary<string, string?>(tags.Length + (instrumentTags?.Length ?? 0), StringComparer.Ordinal);

        // Apply the instrument's own tags first so that a measurement tag of the same
        // name takes precedence over an instrument tag, matching OpenTelemetry semantics.
        if (instrumentTags is not null)
        {
            foreach (var tag in instrumentTags)
            {
                result[tag.Key] = InstrumentState.FormatTagValue(tag.Value);
            }
        }

        foreach (var tag in tags)
        {
            result[tag.Key] = InstrumentState.FormatTagValue(tag.Value);
        }

        return result;
    }
}
