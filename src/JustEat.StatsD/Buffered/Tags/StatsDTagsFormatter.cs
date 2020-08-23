using System;
using System.Collections.Generic;

namespace JustEat.StatsD.Buffered.Tags
{
    internal sealed class StatsDTagsFormatter : IStatsDTagsFormatter
    {
        private readonly IStatsDTagsFormatter _inner;

        public StatsDTagsFormatter(TagsStyle tagsStyle)
        {
            _inner = tagsStyle switch
            {
                TagsStyle.Disabled => new NoOpFormatter(),
                TagsStyle.DataDog => new DataDogFormatter(),
                TagsStyle.InfluxDb => new InfluxDbFormatter(),
                TagsStyle.Librato => new LibratoFormatter(),
                TagsStyle.SignalFx => new SignalFxFormatter(),
                _ => throw new ArgumentOutOfRangeException(nameof(tagsStyle))
            };
        }

        public int GetTagsBufferSize(in IDictionary<string, string?>? tags)
        {
            return _inner.GetTagsBufferSize(tags);
        }

        public bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags)
        {
            return _inner.TryWriteBucketNameTagsIfNeeded(ref buffer, tags);
        }

        public bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags)
        {
            return _inner.TryWriteSuffixTagsIfNeeded(ref buffer, tags);
        }
    }
}
