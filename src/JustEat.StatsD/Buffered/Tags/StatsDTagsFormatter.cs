using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.Buffered.Tags
{
    /// <summary>
    /// A template class to format StatsD tags with configured values.
    /// </summary>
    public abstract class StatsDTagsFormatter : IStatsDTagsFormatter
    {
        private readonly byte[] _utf8Prefix;
        private readonly byte[] _utf8Suffix;
        private readonly bool _areBucketNameTags;
        private readonly string _tagsSeparator;
        private readonly string _keyValueSeparator;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDTagsFormatter"/> class.
        /// </summary>
        /// <param name="prefix">The characters before the tag(s).</param>
        /// <param name="suffix">The characters after the tag(s).</param>
        /// <param name="areBucketNameTags"><c>true</c> if the tags are placed along with the bucket name. <c>false</c> if tags are at the end of the message.</param>
        /// <param name="tagsSeparator">The characters between tags.</param>
        /// <param name="keyValueSeparator">The characters between the tag key and its value.</param>
        protected StatsDTagsFormatter(string prefix, string suffix, bool areBucketNameTags, string tagsSeparator, string keyValueSeparator)
        {
            _utf8Prefix = string.IsNullOrWhiteSpace(prefix) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(prefix);
            _utf8Suffix = string.IsNullOrWhiteSpace(suffix) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(suffix);
            _areBucketNameTags = areBucketNameTags;
            _tagsSeparator = tagsSeparator;
            _keyValueSeparator = keyValueSeparator;
        }

        /// <inheritdoc />
        public virtual int GetTagsBufferSize(in IDictionary<string, string?>? tags)
        {
            const int NoTagsSize = 0;
            if (!AreTagsPresent(tags))
            {
                return NoTagsSize;
            }

            return _utf8Prefix.Length
                + Encoding.UTF8.GetByteCount(GetFormattedTags(tags!))
                + _utf8Suffix.Length;
        }
        
        /// <inheritdoc />
        public virtual bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags) =>
            TryWriteTagsIfNeeded(ref buffer, tags, _areBucketNameTags);
        
        /// <inheritdoc />
        public virtual bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags) =>
            TryWriteTagsIfNeeded(ref buffer, tags, !_areBucketNameTags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags, bool include)
        {
            if (include && AreTagsPresent(tags))
            {
                return buffer.TryWriteBytes(_utf8Prefix)
                       && buffer.TryWriteUtf8String(GetFormattedTags(tags!))
                       && buffer.TryWriteBytes(_utf8Suffix);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetFormattedTags(IDictionary<string, string?> tags) =>
            string.Join(_tagsSeparator,tags.Select(tag => GetFormattedTag(tag)));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetFormattedTag(KeyValuePair<string, string?> tag) =>
            tag.Value == null
                ? tag.Key
                : tag.Key + _keyValueSeparator + tag.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreTagsPresent(IDictionary<string, string?>? tags) =>
            tags != null && tags.Count > 0;
    }
}
