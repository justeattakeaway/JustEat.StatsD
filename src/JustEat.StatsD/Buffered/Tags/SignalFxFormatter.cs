namespace JustEat.StatsD.Buffered.Tags
{
    internal sealed class SignalFxFormatter : BaseTagsFormatter
    {
        // {<optional> "[" + tag1=value1,tag2,tag3=value + "]"}
        private const string Prefix = "[";
        private const string Suffix = "]";
        private const TagsLocation Location = TagsLocation.BucketName;
        private const string TagsSeparator = ",";
        private const string KeyValueSeparator = "=";
        
        public SignalFxFormatter()
            : base(Prefix, Suffix, Location, TagsSeparator, KeyValueSeparator)
        {
        }
    }
}
