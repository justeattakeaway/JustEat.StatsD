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
        private readonly string _prefix;
        private readonly string _suffix;
        private readonly string _tagsSeparator;
        private readonly string _keyValueSeparator;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="StatsDTagsFormatter"/> class.
        /// </summary>
        /// <param name="prefix">The characters before the tag(s).</param>
        /// <param name="suffix">The characters after the tag(s).</param>
        /// <param name="areTrailing"><c>true</c> if the tags at the end of the message. <c>false</c> if tags are placed along with the bucket name.</param>
        /// <param name="tagsSeparator">The characters between tags.</param>
        /// <param name="keyValueSeparator">The characters between the tag key and its value.</param>
        protected StatsDTagsFormatter(string prefix, string suffix, bool areTrailing, string tagsSeparator, string keyValueSeparator)
        {
            _prefix = prefix ?? string.Empty;
            _suffix = suffix ?? string.Empty;
            AreTrailing = areTrailing;
            _tagsSeparator = tagsSeparator ?? string.Empty;
            _keyValueSeparator = keyValueSeparator ?? string.Empty;
        }
        
        /// <inheritdoc />
        public bool AreTrailing { get; }

        /// <inheritdoc />
        public virtual int GetTagsBufferSize(in IDictionary<string, string?>? tags)
        {
            const int NoTagsSize = 0;
            if (!AreTagsPresent(tags))
            {
                return NoTagsSize;
            }

            return _prefix.Length
                + Encoding.UTF8.GetByteCount(GetFormattedTags(tags!))
                + _suffix.Length;
        }
        
        /// <inheritdoc />
        public virtual string GetFormattedTags(in IDictionary<string, string?>? tags)
        {
            if (AreTagsPresent(tags))
            {
                return _prefix + GetFormattedTags(tags!) + _suffix;
            }

            return string.Empty;
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
