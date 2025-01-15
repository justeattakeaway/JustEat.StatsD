namespace JustEat.StatsD.TagsFormatters;

/// <summary>
/// Formats StatsD tags for GraphiteDB.
/// Tags placed right after the bucket name with format: <code>";" + tag1=value1;tag2;tag3=value</code>.
/// </summary>
internal sealed class GraphiteDbTagsFormatter : StatsDTagsFormatter
{
    private const string Prefix = ";";
    private const bool AreTrailingTags = false;
    private const string TagsSeparator = ";";
    private const string KeyValueSeparator = "=";

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphiteDbTagsFormatter"/> class.
    /// </summary>
    public GraphiteDbTagsFormatter()
        : base(new StatsDTagsFormatterConfiguration
        {
            Prefix = Prefix,
            Suffix = string.Empty,
            AreTrailing = AreTrailingTags,
            TagsSeparator = TagsSeparator,
            KeyValueSeparator = KeyValueSeparator,
        })
    {
    }
}
