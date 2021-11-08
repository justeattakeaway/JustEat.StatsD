namespace JustEat.StatsD.TagsFormatters;

/// <summary>
/// A class representing the configuration options for <see cref="StatsDTagsFormatter"/>.
/// </summary>
public class StatsDTagsFormatterConfiguration
{
    /// <summary>
    /// Gets or sets the character(s) before the tag(s).
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character(s) after the tag(s).
    /// </summary>
    public string Suffix { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating if the tags at the end of the message or otherwise, if tags are placed along with the bucket name.
    /// </summary>
    public bool AreTrailing { get; set; }

    /// <summary>
    /// Gets or sets the character(s) between tags.
    /// </summary>
    public string TagsSeparator { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the character(s) between the tag key and its value.
    /// </summary>
    public string KeyValueSeparator { get; set; } = string.Empty;
}
