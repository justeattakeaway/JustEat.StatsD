namespace JustEat.StatsD.Buffered.Tags
{
    /// <summary>
    /// Formats StatsD tags for SignalFX.
    /// Tags placed right after the bucket name with format: <code>"[" + tag1=value1,tag2,tag3=value + "]"</code>.
    /// </summary>
    public sealed class SignalFxTagsFormatter : StatsDTagsFormatter
    {
        private const string Prefix = "[";
        private const string Suffix = "]";
        private const bool AreBucketNameTags = true;
        private const string TagsSeparator = ",";
        private const string KeyValueSeparator = "=";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalFxTagsFormatter"/> class.
        /// </summary>
        public SignalFxTagsFormatter()
            : base(Prefix, Suffix, AreBucketNameTags, TagsSeparator, KeyValueSeparator)
        {
        }
    }
}
