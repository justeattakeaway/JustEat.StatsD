using System;
using System.Collections.Generic;

namespace JustEat.StatsD.TagsFormatters
{
    internal sealed class NoOpTagsFormatter : IStatsDTagsFormatter
    {
        public bool AreTrailing { get; }

        public int GetTagsBufferSize(in Dictionary<string, string?> tags) => 0;
        
        public ReadOnlySpan<char> FormatTags(in Dictionary<string, string?> tags) => ReadOnlySpan<char>.Empty;
    }
}
