using System.Runtime.CompilerServices;
using System.Text;
using JustEat.StatsD.TagsFormatters;

namespace JustEat.StatsD.Buffered;

internal sealed class StatsDUtf8Formatter
{
    private readonly byte[] _utf8Prefix;
    private readonly IStatsDTagsFormatter _tagsFormatter;

    public StatsDUtf8Formatter(string prefix, IStatsDTagsFormatter tagsFormatter)
    {
        _utf8Prefix = string.IsNullOrWhiteSpace(prefix) ?
            Array.Empty<byte>() :
            Encoding.UTF8.GetBytes(prefix + ".");
        _tagsFormatter = tagsFormatter ?? new NoOpTagsFormatter();
    }

    public int GetMaxBufferSize(in StatsDMessage msg)
    {
        const int MaxSerializedDoubleSymbols = 32;
        const int ColonBytes = 1;

        const int MaxMessageKindSuffixSize = 3;
        const int MaxSamplingSuffixSize = 2;

        return _utf8Prefix.Length
               + Encoding.UTF8.GetByteCount(msg.StatBucket)
               + ColonBytes
               + MaxSerializedDoubleSymbols
               + MaxMessageKindSuffixSize
               + MaxSamplingSuffixSize
               + MaxSerializedDoubleSymbols
               + GetTagsBufferSize(msg.Tags);
    }

    public bool TryFormat(in StatsDMessage msg, double sampleRate, Span<byte> destination, out int written)
    {
        var buffer = new Buffer<byte>(destination);

        bool isFormattingSuccessful =
              TryWriteBucketNameWithColon(ref buffer, msg)
           && TryWritePayloadWithMessageKindSuffix(ref buffer, msg)
           && TryWriteSampleRateIfNeeded(ref buffer, sampleRate)
           && TryWriteTrailingTagsIfNeeded(ref buffer, msg.Tags);

        written = isFormattingSuccessful ? buffer.Written : 0;
        return isFormattingSuccessful;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWriteBucketNameWithColon(ref Buffer<byte> buffer, in StatsDMessage msg)
    {
        // prefix + msg.Bucket + {optional msg.Tags} + ":"

        return buffer.TryWrite(_utf8Prefix)
            && buffer.TryWriteUtf8String(msg.StatBucket)
            && TryWriteBucketNameTagsIfNeeded(ref buffer, msg.Tags)
            && buffer.TryWrite((byte)':');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePayloadWithMessageKindSuffix(ref Buffer<byte> buffer, in StatsDMessage msg)
    {
        // msg.Value + (<oneOf> "|ms", "|c", "|g")

        var integralMagnitude = (long)msg.Magnitude;

        switch (msg.MessageKind)
        {
            case StatsDMessageKind.Counter:
                {
                    return buffer.TryWriteInt64(integralMagnitude)
                        && buffer.TryWrite((byte)'|', (byte)'c');
                }
            case StatsDMessageKind.Timing:
                {
                    return buffer.TryWriteInt64(integralMagnitude)
                        && buffer.TryWrite((byte)'|', (byte)'m', (byte)'s');
                }
            case StatsDMessageKind.Gauge:
                {
                    // check if magnitude is integral, integers are written significantly faster
                    bool isMagnitudeIntegral = msg.Magnitude == integralMagnitude;

                    var successSoFar = isMagnitudeIntegral ?
                        buffer.TryWriteInt64(integralMagnitude) :
                        buffer.TryWriteDouble(msg.Magnitude);

                    return successSoFar && buffer.TryWrite((byte)'|', (byte)'g');
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(msg));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWriteSampleRateIfNeeded(ref Buffer<byte> buffer, double sampleRate)
    {
        // {<optional> "|@" + sampleRate}

        if (sampleRate < 1.0 && sampleRate > 0.0)
        {
            return buffer.TryWrite((byte)'|', (byte)'@')
                && buffer.TryWriteDouble(sampleRate);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetTagsBufferSize(in Dictionary<string, string?>? tags) =>
        AreTagsPresent(tags)
            ? _tagsFormatter.GetTagsBufferSize(tags!)
            : 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWriteBucketNameTagsIfNeeded(ref Buffer<byte> buffer, in Dictionary<string, string?>? tags) =>
        !AreTagsPresent(tags) ||
        _tagsFormatter.AreTrailing ||
        buffer.TryWriteUtf8Chars(_tagsFormatter.FormatTags(tags!));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWriteTrailingTagsIfNeeded(ref Buffer<byte> buffer, in Dictionary<string, string?>? tags) =>
        !AreTagsPresent(tags) ||
        !_tagsFormatter.AreTrailing ||
        buffer.TryWriteUtf8Chars(_tagsFormatter.FormatTags(tags!));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreTagsPresent(in Dictionary<string, string?>? tags) =>
        tags != null && tags.Count > 0;
}
