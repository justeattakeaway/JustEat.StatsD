namespace JustEat.StatsD.TagsFormatters
{
    /// <summary>
    /// Formats StatsD tags for InfluxDB.
    /// Tags placed right after the bucket name with format: <code>"," + tag1=value1,tag2,tag3=value</code>.
    /// </summary>
    public sealed class InfluxDbTagsFormatter : StatsDTagsFormatter
    {
        private const string Prefix = ",";
        private const bool AreTrailingTags = false;
        private const string TagsSeparator = ",";
        private const string KeyValueSeparator = "=";

        /// <summary>
        /// Initializes a new instance of the <see cref="InfluxDbTagsFormatter"/> class.
        /// </summary>
        public InfluxDbTagsFormatter()
            : base(Prefix, string.Empty, AreTrailingTags, TagsSeparator, KeyValueSeparator)
        {
        }
    }
}
