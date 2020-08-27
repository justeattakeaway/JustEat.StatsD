using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JustEat.StatsD.Buffered;

namespace JustEat.StatsD.TagsFormatters
{
    /// <summary>
    /// A template class to format StatsD tags with configured values.
    /// </summary>
    public abstract class StatsDTagsFormatter : IStatsDTagsFormatter
    {
        private const int SafeUdpPacketSize = 512;

        [ThreadStatic]
        private static char[]? _buffer;
        private static char[] Buffer => _buffer ??= new char[SafeUdpPacketSize];

        private readonly char[] _prefix;
        private readonly char[] _suffix;
        private readonly char[] _tagsSeparator;
        private readonly char[] _keyValueSeparator;
        private readonly int _prefixSize;
        private readonly int _suffixSize;
        private readonly int _tagsSeparatorSize;
        private readonly int _keyValueSeparatorSize;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDTagsFormatter"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        protected StatsDTagsFormatter(StatsDTagsFormatterConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _prefix = configuration.Prefix?.ToArray() ?? Array.Empty<char>();
            _suffix = configuration.Suffix?.ToArray() ?? Array.Empty<char>();
            AreTrailing = configuration.AreTrailing;
            _tagsSeparator = configuration.TagsSeparator?.ToArray() ?? Array.Empty<char>();
            _keyValueSeparator = configuration.KeyValueSeparator?.ToArray() ?? Array.Empty<char>();
            _prefixSize = Encoding.UTF8.GetByteCount(_prefix);
            _suffixSize = Encoding.UTF8.GetByteCount(_suffix);
            _tagsSeparatorSize = Encoding.UTF8.GetByteCount(_tagsSeparator);
            _keyValueSeparatorSize = Encoding.UTF8.GetByteCount(_keyValueSeparator);
        }
        
        /// <inheritdoc />
        public bool AreTrailing { get; }

        /// <inheritdoc />
        public virtual int GetTagsBufferSize(in Dictionary<string, string?> tags)
        {
            const int NoTagsSize = 0;
            if (!AreTagsPresent(tags))
            {
                return NoTagsSize;
            }

            return _prefixSize
                + GetTagsSize(tags)
                + _suffixSize;
        }
        
        /// <inheritdoc />
        public virtual ReadOnlySpan<char> FormatTags(in Dictionary<string, string?> tags)
        {
            if (!AreTagsPresent(tags))
            {
                return ReadOnlySpan<char>.Empty;
            }

            char[] destination = Buffer;
            if (!TryFormatTags(destination, tags, out int written))
            {
                var newSize = GetTagsBufferSize(tags);
                _buffer = new char[newSize];
                destination = Buffer;

                if (!TryFormatTags(destination, tags, out written))
                {
                    throw new FormatException("Failed to format tags.");
                }
            }

            return new ArraySegment<char>(destination, 0, written);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryFormatTags(Span<char> destination, in Dictionary<string, string?> tags, out int written)
        {
            var buffer = new Buffer<char>(destination);
            bool isFormattingSuccessful = buffer.TryWrite(_prefix)
                && TryWriteTags(ref buffer, tags)
                && buffer.TryWrite(_suffix);

            written = isFormattingSuccessful ? buffer.Written : 0;

            return isFormattingSuccessful;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteTags(ref Buffer<char> buffer, in Dictionary<string, string?> tags)
        {
            var index = 0;
            foreach (var tag in tags)
            {
                var isFormattingSuccessful = TryWriteTag(ref buffer, tag)
                                             && TryWriteTagsSeparator(ref buffer, index++, tags);
                if (!isFormattingSuccessful)
                {
                    return false;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteTagsSeparator(ref Buffer<char> buffer, int index, in Dictionary<string, string?> tags) =>
            !IsLastTag(index, tags)
                ? buffer.TryWrite(_tagsSeparator)
                : true;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsLastTag(int index, in Dictionary<string, string?> tags) =>
            index == tags.Count - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteTag(ref Buffer<char> buffer, in KeyValuePair<string, string?> tag) =>
            buffer.TryWriteString(tag.Key)
            && TryWriteTagValueIfNeeded(ref buffer, tag);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteTagValueIfNeeded(ref Buffer<char> buffer, KeyValuePair<string, string?> tag) =>
            tag.Value != null
                ? buffer.TryWrite(_keyValueSeparator) && buffer.TryWriteString(tag.Value!)
                : true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreTagsPresent(in Dictionary<string, string?>? tags) =>
            tags != null && tags.Count > 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTagsSize(in Dictionary<string, string?> tags)
        {
            var tagsSize = 0;
            foreach (KeyValuePair<string, string?> tag in tags)
            {
                tagsSize += GetTagSize(tag);
            }

            tagsSize += (tags.Count - 1) * _tagsSeparatorSize;

            return tagsSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTagSize(KeyValuePair<string, string?> tag) =>
            Encoding.UTF8.GetByteCount(tag.Key) + GetTagValueSize(tag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTagValueSize(KeyValuePair<string, string?> tag) =>
            tag.Value != null
                ? Encoding.UTF8.GetByteCount(tag.Value) + _keyValueSeparatorSize
                : 0;
    }
}
