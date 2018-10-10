namespace JustEat.StatsD
{
    /// <summary>
    /// Defines an interface for publishing counters, gauges and timers to statsD.
    /// </summary>
    public interface IStatsDPublisher
    {
        /// <summary>
        /// Publishes a counter for the specified bucket and value.
        /// </summary>
        /// <param name="value">The value to increment the counter by.</param>
        /// <param name="sampleRate">The sample rate for the counter.</param>
        /// <param name="bucket">The bucket to increment the counter for.</param>
        void Increment(long value, double sampleRate, string bucket);

        /// <summary>
        /// Publishes a gauge for the specified bucket and value.
        /// </summary>
        /// <param name="value">The value to publish for the gauge.</param>
        /// <param name="sampleRate">The sample rate for the gauge.</param>
        /// <param name="bucket">The bucket to publish the gauge for.</param>
        void Gauge(double value, string bucket);

        /// <summary>
        /// Publishes a timer for the specified bucket and value.
        /// </summary>
        /// <param name="duration">The value to publish for the timer.</param>
        /// <param name="sampleRate">The sample rate for the timer.</param>
        /// <param name="bucket">The bucket to publish the timer for.</param>
        void Timing(long duration, double sampleRate, string bucket);
    }
}
