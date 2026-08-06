using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace JustEat.StatsD.Diagnostics;

/// <summary>
/// A class holding the state associated with an enabled instrument. This class cannot be inherited.
/// </summary>
internal sealed class InstrumentState
{
    // The maximum number of measurement tags for which the order used
    // to build a lookup key is computed without allocating an array.
    private const int MaxStackAllocTags = 16;

    private readonly ConcurrentDictionary<string, CumulativeValue>? _values;

    private InstrumentState(
        StatsDMetricKind kind,
        string bucket,
        double scaleFactor,
        double sampleRate,
        bool accumulatesResidual,
        KeyValuePair<string, object?>[]? instrumentTags)
    {
        Kind = kind;
        Bucket = bucket;
        ScaleFactor = scaleFactor;
        SampleRate = sampleRate;
        AccumulatesResidual = accumulatesResidual;
        InstrumentTags = instrumentTags;

        if (kind == StatsDMetricKind.CumulativeCounter || accumulatesResidual)
        {
            _values = new ConcurrentDictionary<string, CumulativeValue>(concurrencyLevel: 1, capacity: 1, StringComparer.Ordinal);
        }
    }

    internal StatsDMetricKind Kind { get; }

    internal string Bucket { get; }

    internal double ScaleFactor { get; }

    internal double SampleRate { get; }

    /// <summary>
    /// Gets a value indicating whether the (non-cumulative) counter accumulates the fractional
    /// part of its deltas so that fractional measurements are not lost to integer truncation.
    /// </summary>
    internal bool AccumulatesResidual { get; }

    /// <summary>
    /// Gets the tags associated with the instrument itself, or <see langword="null"/> if it has none.
    /// </summary>
    internal KeyValuePair<string, object?>[]? InstrumentTags { get; }

    internal static InstrumentState? TryCreate(Instrument instrument, StatsDMetricsOptions options)
    {
        var type = instrument.GetType();
        var genericType = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

        StatsDMetricKind kind;
        double scaleFactor = 1;
        bool accumulatesResidual = false;

        if (genericType == typeof(Counter<>) || genericType == typeof(UpDownCounter<>))
        {
            // Both instruments report deltas, which is exactly what a StatsD counter expects.
            kind = StatsDMetricKind.Counter;

            // StatsD counters are integer-valued, so a counter of a floating-point type must
            // accumulate the fractional part of its deltas rather than round each one in
            // isolation, which would otherwise silently drop deltas smaller than 0.5.
            var valueType = type.GetGenericArguments()[0];
            accumulatesResidual = valueType == typeof(double) || valueType == typeof(float) || valueType == typeof(decimal);
        }
        else if (genericType == typeof(Histogram<>))
        {
            // StatsD timers are the only aggregating StatsD metric, so all
            // histograms are published as timers. Durations that follow the
            // OpenTelemetry conventions are recorded in seconds, whereas
            // StatsD timers are conventionally milliseconds.
            kind = StatsDMetricKind.Timing;

            if (options.ConvertSecondsToMilliseconds && string.Equals(instrument.Unit, "s", StringComparison.Ordinal))
            {
                scaleFactor = 1000;
            }
        }
        else if (genericType == typeof(Gauge<>) || genericType == typeof(ObservableGauge<>) || genericType == typeof(ObservableUpDownCounter<>))
        {
            // An ObservableUpDownCounter reports its current total (e.g. the length
            // of a queue), which has the semantics of a StatsD gauge, not a counter.
            kind = StatsDMetricKind.Gauge;
        }
        else if (genericType == typeof(ObservableCounter<>))
        {
            // ObservableCounter reports cumulative totals, which must be
            // converted to deltas before being published as StatsD counters.
            kind = StatsDMetricKind.CumulativeCounter;
        }
        else
        {
            return null;
        }

        string bucket = options.BucketNameProvider?.Invoke(instrument) ?? DefaultBucketName(instrument);

        if (string.IsNullOrEmpty(bucket))
        {
            return null;
        }

        // Sanitized regardless of where the name came from so that a custom bucket name
        // cannot contain characters that the StatsD protocol treats as separators.
        bucket = Sanitize(bucket);

        double sampleRate = options.SampleRateProvider?.Invoke(instrument) ?? 1;

        // A sample rate outside the range that StatsD supports would cause the
        // instrument's measurements to be silently discarded, so ignore it.
        if (double.IsNaN(sampleRate) || sampleRate <= 0 || sampleRate > 1)
        {
            sampleRate = 1;
        }

        // Tags supplied when the instrument was created are not delivered in the measurement
        // callback, so capture them here to merge with the per-measurement tags when publishing.
        var instrumentTags = instrument.Tags?.ToArray();

        if (instrumentTags is { Length: 0 })
        {
            instrumentTags = null;
        }

        return new InstrumentState(kind, bucket, scaleFactor, sampleRate, accumulatesResidual, instrumentTags);
    }

    /// <summary>
    /// Accumulates a counter delta, returning the whole-number part to publish and carrying the
    /// fractional remainder over to the next measurement, keyed by the measurement's tags.
    /// </summary>
    internal long NextCounterDelta(string tagsKey, double delta)
    {
        var state = _values!.GetOrAdd(tagsKey, static (_) => new CumulativeValue());

        lock (state)
        {
            state.Residual += delta;

            long result = (long)state.Residual;
            state.Residual -= result;

            return result;
        }
    }

    /// <summary>
    /// Converts a cumulative integral total into the delta to publish, keyed by the measurement's tags.
    /// </summary>
    /// <remarks>
    /// The total is tracked as a <see cref="long"/> rather than a <see cref="double"/> so that a
    /// counter which grows beyond the range that a <see cref="double"/> can represent exactly
    /// (2^53) continues to produce accurate deltas rather than quantized ones.
    /// </remarks>
    internal long NextDelta(string tagsKey, long value)
    {
        var cumulative = _values!.GetOrAdd(tagsKey, static (_) => new CumulativeValue());

        lock (cumulative)
        {
            long delta = cumulative.HasValue ? value - cumulative.IntegralValue : value;

            if (delta < 0)
            {
                // The counter appears to have been reset, so publish the new total.
                delta = value;
            }

            cumulative.IntegralValue = value;
            cumulative.HasValue = true;

            return delta;
        }
    }

    /// <summary>
    /// Converts a cumulative total into the delta to publish, keyed by the measurement's tags.
    /// </summary>
    internal long NextDelta(string tagsKey, double value)
    {
        var cumulative = _values!.GetOrAdd(tagsKey, static (_) => new CumulativeValue());

        lock (cumulative)
        {
            double delta = cumulative.HasValue ? value - cumulative.Value : value;

            if (delta < 0)
            {
                // The counter appears to have been reset, so publish the new total.
                delta = value;
            }

            cumulative.Value = value;
            cumulative.HasValue = true;

            // Accumulate any fractional part so that non-integral
            // totals do not drift as deltas are truncated.
            cumulative.Residual += delta;

            long result = (long)cumulative.Residual;
            cumulative.Residual -= result;

            return result;
        }
    }

    internal static string TagsKey(ReadOnlySpan<KeyValuePair<string, object?>> tags)
    {
        if (tags.IsEmpty)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        if (tags.Length == 1)
        {
            return AppendTag(builder, tags[0]).ToString();
        }

        // The same logical set of tags can be recorded with the tags in a different order by
        // different call sites, so they are ordered by name to prevent such measurements being
        // tracked as if they belonged to two different time series.
        Span<int> order = stackalloc int[MaxStackAllocTags];

        if (tags.Length > MaxStackAllocTags)
        {
            order = new int[tags.Length];
        }
        else
        {
            order = order.Slice(0, tags.Length);
        }

        for (int i = 0; i < order.Length; i++)
        {
            order[i] = i;
        }

        // An insertion sort is used as instruments have few tags in practice.
        for (int i = 1; i < order.Length; i++)
        {
            int current = order[i];
            int j = i - 1;

            while (j > -1 && string.CompareOrdinal(tags[order[j]].Key, tags[current].Key) > 0)
            {
                order[j + 1] = order[j];
                j--;
            }

            order[j + 1] = current;
        }

        foreach (int index in order)
        {
            AppendTag(builder, tags[index]);
        }

        return builder.ToString();
    }

    private static StringBuilder AppendTag(StringBuilder builder, KeyValuePair<string, object?> tag)
    {
        string? value = FormatTagValue(tag.Value);

        builder.Append(tag.Key)
               .Append('\u001e');

        // A null tag value is marked explicitly so that it does not produce
        // the same key as a tag whose value is an empty string.
        if (value is null)
        {
            builder.Append('\u0000');
        }
        else
        {
            builder.Append('\u0001')
                   .Append(value);
        }

        return builder.Append('\u001f');
    }

    internal static string? FormatTagValue(object? value)
    {
        return value switch
        {
            null => null,
            string text => text,
            bool flag => flag ? "true" : "false",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }

    private static string DefaultBucketName(Instrument instrument)
    {
        return instrument.Meter.Name.Length > 0 ?
            instrument.Meter.Name + "." + instrument.Name :
            instrument.Name;
    }

    private static string Sanitize(string bucket)
    {
        char[]? sanitized = null;

        for (int i = 0; i < bucket.Length; i++)
        {
            char ch = bucket[i];

            if (ch is ':' or '|' or '@' or '#' or '\n' || char.IsWhiteSpace(ch))
            {
                sanitized ??= bucket.ToCharArray();
                sanitized[i] = '_';
            }
        }

        return sanitized is null ? bucket : new string(sanitized);
    }

    // An instrument's measurements are always of a single type, so only the value
    // field matching that type's code path is ever used for a given instrument.
    private sealed class CumulativeValue
    {
        internal double Value;
        internal long IntegralValue;
        internal double Residual;
        internal bool HasValue;
    }
}
