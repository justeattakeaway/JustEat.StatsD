using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace JustEat.StatsD.Buffered.Tags
{
    internal sealed class DataDogFormatter : IStatsDTagsFormatter
    {
        public int GetTagsBufferSize(in IDictionary<string, string?>? tags)
        {
            const int TaggingSuffixSize = 2;
            return TaggingSuffixSize
                + Encoding.UTF8.GetByteCount(GetFormattedTags(tags));
        }

        public bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags)
        {
            return true;
        }

        public bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags)
        {
            // {<optional> "|#" + tag1:value1,tag2,tag3:value}
            
            if (tags == null || !tags.Any())
            {
                return true;
            }

            return buffer.TryWriteBytes((byte)'|', (byte)'#')
                       && buffer.TryWriteUtf8String(GetFormattedTags(tags));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetFormattedTags(IDictionary<string, string?>? tags)
        {
            if (tags == null || !tags.Any())
            {
                return string.Empty;
            }

            return string.Join(",", tags.Select(tag => tag.Value == null ? tag.Key : $"{tag.Key}:{tag.Value}"));
        }
    }
}
