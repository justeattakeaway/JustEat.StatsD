using System.Collections.Generic;

namespace JustEat.StatsD.Buffered.Tags
{
    internal interface IStatsDTagsFormatter
    {
        int GetTagsBufferSize(in IDictionary<string, string?>? tags);

        bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags);

        bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags);
    }
}
