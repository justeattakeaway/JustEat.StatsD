namespace JustEat.StatsD.Buffered.Tags
{
    internal sealed class LibratoFormatter : BaseTagsFormatter
    {
        // {<optional> "#" + tag1=value1,tag2,tag3=value}
        private const string Prefix = "#";
        private const TagsLocation Location = TagsLocation.BucketName;
        private const string TagsSeparator = ",";
        private const string KeyValueSeparator = "=";
        
        public LibratoFormatter()
            : base(Prefix, string.Empty, Location, TagsSeparator, KeyValueSeparator)
        {
        }
    }
}
