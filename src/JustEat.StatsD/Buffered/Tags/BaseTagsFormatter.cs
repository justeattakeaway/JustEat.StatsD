using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.Buffered.Tags
{
    internal abstract class BaseTagsFormatter : IStatsDTagsFormatter
    {
        private readonly byte[] _utf8Prefix;
        private readonly byte[] _utf8Suffix;
        private readonly TagsLocation _location;
        private readonly string _tagsSeparator;
        private readonly string _keyValueSeparator;

        protected BaseTagsFormatter(string prefix, string suffix, TagsLocation location, string tagsSeparator, string keyValueSeparator)
        {
            _utf8Prefix = string.IsNullOrWhiteSpace(prefix) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(prefix);
            _utf8Suffix = string.IsNullOrWhiteSpace(suffix) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(suffix);
            _location = location;
            _tagsSeparator = tagsSeparator;
            _keyValueSeparator = keyValueSeparator;
        }

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

        public virtual bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags) =>
            TryWriteTagsIfNeeded(ref buffer, tags, TagsLocation.BucketName);

        public virtual bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags) =>
            TryWriteTagsIfNeeded(ref buffer, tags, TagsLocation.Suffix);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryWriteTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags, TagsLocation location)
        {
            if (AreTagsPresent(tags) && _location == location)
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
                : $"{tag.Key}{this._keyValueSeparator}{tag.Value}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreTagsPresent(IDictionary<string, string?>? tags) =>
            tags != null && tags.Any();
    }
}
