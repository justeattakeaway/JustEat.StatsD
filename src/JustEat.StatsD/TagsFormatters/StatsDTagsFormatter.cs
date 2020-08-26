using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.TagsFormatters
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
        /// <param name="configuration">The configuration.</param>
        protected StatsDTagsFormatter(StatsDTagsFormatterConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _prefix = configuration.Prefix ?? string.Empty;
            _suffix = configuration.Suffix ?? string.Empty;
            AreTrailing = configuration.AreTrailing;
            _tagsSeparator = configuration.TagsSeparator ?? string.Empty;
            _keyValueSeparator = configuration.KeyValueSeparator ?? string.Empty;
        }
        
        /// <inheritdoc />
        public bool AreTrailing { get; }

        /// <inheritdoc />
        public virtual int GetTagsBufferSize(in IDictionary<string, string?> tags)
        {
            const int NoTagsSize = 0;
            if (!AreTagsPresent(tags))
            {
                return NoTagsSize;
            }

            return _prefix.Length
                + Encoding.UTF8.GetByteCount(FormatTags(tags!))
                + _suffix.Length;
        }
        
        /// <inheritdoc />
        public virtual string FormatTags(in IDictionary<string, string?> tags)
        {
            if (AreTagsPresent(tags))
            {
                return _prefix + FormatTags(tags!) + _suffix;
            }

            return string.Empty;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormatTags(IDictionary<string, string?> tags) =>
            string.Join(_tagsSeparator,tags.Select(FormatTags));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string FormatTags(KeyValuePair<string, string?> tag) =>
            tag.Value == null
                ? tag.Key
                : tag.Key + _keyValueSeparator + tag.Value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreTagsPresent(IDictionary<string, string?>? tags) =>
            tags != null && tags.Count > 0;
    }
}
