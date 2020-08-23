using System.Collections.Generic;

namespace JustEat.StatsD.Buffered.Tags
{
    internal sealed class NoOpFormatter : IStatsDTagsFormatter
    {
        public int GetTagsBufferSize(in IDictionary<string, string?>? tags)
        {
            return 0;
        }

        public bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags)
        {
            return true;
        }

        public bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags)
        {
            return true;
        }
    }
}
