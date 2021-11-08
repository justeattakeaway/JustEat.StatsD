namespace JustEat.StatsD.TagsFormatters;

/// <summary>
/// Formats StatsD tags for SignalFX.
/// Tags placed right after the bucket name with format: <code>"[" + tag1=value1,tag2,tag3=value + "]"</code>.
/// </summary>
internal sealed class SignalFxTagsFormatter : StatsDTagsFormatter
{
    private const string Prefix = "[";
    private const string Suffix = "]";
    private const bool AreTrailingTags = false;
    private const string TagsSeparator = ",";
    private const string KeyValueSeparator = "=";

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalFxTagsFormatter"/> class.
    /// </summary>
    public SignalFxTagsFormatter()
        : base(new StatsDTagsFormatterConfiguration
        {
            Prefix = Prefix,
            Suffix = Suffix,
            AreTrailing = AreTrailingTags,
            TagsSeparator = TagsSeparator,
            KeyValueSeparator = KeyValueSeparator,
        })
    {
    }
}
