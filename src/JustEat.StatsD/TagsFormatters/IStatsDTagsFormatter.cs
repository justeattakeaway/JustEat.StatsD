namespace JustEat.StatsD.TagsFormatters;

/// <summary>
/// Defines an interface for formatting tags for extended StatsD specification.
/// </summary>
public interface IStatsDTagsFormatter
{
    /// <summary>
    /// Gets a value indicating whether the tags are at the end of the message or otherwise, they are placed with the bucket name.
    /// </summary>
    bool AreTrailing { get; }

    /// <summary>
    /// Calculates the buffer size to write tags considering the fixed characters and the size of the tag(s).
    /// </summary>
    /// <param name="tags">The tag(s) included.</param>
    /// <returns>The amount of bytes dedicated in the buffer for the tags.</returns>
    int GetTagsBufferSize(in Dictionary<string, string?> tags);

    /// <summary>
    /// Calculates the tag(s) formatted to be included in the StatsD message.
    /// </summary>
    /// <param name="tags">The tag(s) included.</param>
    /// <returns>The tag(s) formatted.</returns>
    ReadOnlySpan<char> FormatTags(in Dictionary<string, string?> tags);
}
