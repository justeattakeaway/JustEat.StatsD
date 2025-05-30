namespace JustEat.StatsD.TagsFormatters;

/// <summary>
/// Formats StatsD tags placed at the end of the message. Supported by DataDog, Splunk and AWS CloudWatch.
/// Tags placed at the end of the message with format: <code>"|#" + tag1:value1,tag2,tag3:value</code>.
/// </summary>
internal sealed class TrailingTagsFormatter : StatsDTagsFormatter
{
    private const string Prefix = "|#";
    private const bool AreTrailingTags = true;
    private const string TagsSeparator = ",";
    private const string KeyValueSeparator = ":";

    /// <summary>
    /// Initializes a new instance of the <see cref="TrailingTagsFormatter"/> class.
    /// </summary>
    public TrailingTagsFormatter()
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

    /// <summary>
    /// Gets the singleton instance of the <see cref="TrailingTagsFormatter"/> class.
    /// </summary>
    public static TrailingTagsFormatter Instance { get; } = new TrailingTagsFormatter();
}
