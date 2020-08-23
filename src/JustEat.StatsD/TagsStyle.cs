namespace JustEat.StatsD
{
    /// <summary>
    /// An enumeration defining the tagging formats supported.
    /// <para />
    /// Disabled is the default.
    /// </summary>
    public enum TagsStyle
    {
        /// <summary>
        /// Tags are disabled and they are ignored while sending the message.
        /// </summary>
        Disabled,

        /// <summary>
        /// DataDog's DogStatsD-style tags: metric.name:0|c|#tagName=val,tag2Name.
        /// </summary>
        DataDog,
        
        /// <summary>
        /// InfluxDB-style tags: metric.name,tagName=val,tag2Name:0|c.
        /// </summary>
        InfluxDb,

        /// <summary>
        /// Librato-style tags: metric.name#tagName=val,tag2Name:0|c.
        /// </summary>
        Librato,

        /// <summary>
        /// SignalFX dimensions: metric.name[tagName=val,tag2Name]:0|c.
        /// </summary>
        SignalFx
    }
}
