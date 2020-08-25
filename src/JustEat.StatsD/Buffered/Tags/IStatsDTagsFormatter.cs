using System.Collections.Generic;

namespace JustEat.StatsD.Buffered.Tags
{
    /// <summary>
    /// Defines an interface for formatting tags for extended StatsD specification.
    /// </summary>
    public interface IStatsDTagsFormatter
    {
        /// <summary>
        /// Calculates the buffer size to write tags considering the fixed characters and the size of the tag(s).
        /// </summary>
        /// <param name="tags">The tag(s) included.</param>
        /// <returns>The amount of bytes dedicated in the buffer for the tags.</returns>
        int GetTagsBufferSize(in IDictionary<string, string?>? tags);

        /// <summary>
        /// Writes to the buffer the tag(s) if the are included and they are placed along with the bucket name.
        /// </summary>
        /// <param name="buffer">The buffer where the tag(s) should be written.</param>
        /// <param name="tags">The tag(s) included.</param>
        /// <returns>A value indicating whether the operation was performed successfully.</returns>
        bool TryWriteBucketNameTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags);
        
        /// <summary>
        /// Writes to the buffer the tag(s) if the are included and they are placed at the end of the StatsD message.
        /// </summary>
        /// <param name="buffer">The buffer where the tag(s) should be written.</param>
        /// <param name="tags">The tag(s) included.</param>
        /// <returns>A value indicating whether the operation was performed successfully.</returns>
        bool TryWriteSuffixTagsIfNeeded(ref Buffer buffer, in IDictionary<string, string?>? tags);
    }
}
