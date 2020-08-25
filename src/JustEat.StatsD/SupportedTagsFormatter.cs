using JustEat.StatsD.Buffered.Tags;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class that can be used to retrieve supported <see cref="IStatsDTagsFormatter"/> implementations for some of the major metric providers.
    /// </summary>
    public static class SupportedTagsFormatter
    {
        /// <summary>
        /// Gets the AWS CloudWatch tags formatter.
        /// </summary>
        public static IStatsDTagsFormatter CloudWatch => new TrailingTagsFormatter();

        /// <summary>
        /// Gets the DataDog tags formatter.
        /// </summary>
        public static IStatsDTagsFormatter DataDog => new TrailingTagsFormatter();
        
        /// <summary>
        /// Gets the InfluxDB tags formatter.
        /// </summary>
        public static IStatsDTagsFormatter InfluxDb => new InfluxDbTagsFormatter();
        
        /// <summary>
        /// Gets the Librato tags formatter.
        /// </summary>
        public static IStatsDTagsFormatter Librato => new LibratoTagsFormatter();
        
        /// <summary>
        /// Gets the SignalFX dimensions formatter.
        /// </summary>
        public static IStatsDTagsFormatter SignalFx => new SignalFxTagsFormatter();
        
        /// <summary>
        /// Gets the Splunk tags formatter.
        /// </summary>
        public static IStatsDTagsFormatter Splunk => new TrailingTagsFormatter();
    }
}
