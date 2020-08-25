using System.Collections.Generic;

namespace JustEat.StatsD.Buffered.Tags
{
    internal sealed class NoOpTagsFormatter : IStatsDTagsFormatter
    {
        public bool AreTrailing { get; }

        public int GetTagsBufferSize(in IDictionary<string, string?>? tags) => 0;
        
        public string FormatTags(in IDictionary<string, string?>? tags) => string.Empty;
    }
}
