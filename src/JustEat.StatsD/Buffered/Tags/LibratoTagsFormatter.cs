namespace JustEat.StatsD.Buffered.Tags
{
    /// <summary>
    /// Formats StatsD tags for Librato.
    /// Tags placed right after the bucket name with format: "#" + tag1=value1,tag2,tag3=value
    /// </summary>
    public sealed class LibratoTagsFormatter : StatsDTagsFormatter
    {
        private const string Prefix = "#";
        private const bool AreBucketNameTags = true;
        private const string TagsSeparator = ",";
        private const string KeyValueSeparator = "=";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LibratoTagsFormatter"/> class.
        /// </summary>
        public LibratoTagsFormatter()
            : base(Prefix, string.Empty, AreBucketNameTags, TagsSeparator, KeyValueSeparator)
        {
        }
    }
}
