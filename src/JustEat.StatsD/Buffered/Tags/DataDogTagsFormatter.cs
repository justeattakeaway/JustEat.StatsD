namespace JustEat.StatsD.Buffered.Tags
{
    /// <summary>
    /// Formats StatsD tags for DataDog, Splunk and CloudWatch.
    /// Tags placed at the end of the message with format: <code>"|#" + tag1:value1,tag2,tag3:value</code>.
    /// </summary>
    public sealed class DataDogTagsFormatter : StatsDTagsFormatter
    {
        private const string Prefix = "|#";
        private const bool AreBucketNameTags = false;
        private const string TagsSeparator = ",";
        private const string KeyValueSeparator = ":";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DataDogTagsFormatter"/> class.
        /// </summary>
        public DataDogTagsFormatter()
            : base(Prefix, string.Empty, AreBucketNameTags, TagsSeparator, KeyValueSeparator)
        {
        }
    }
}
