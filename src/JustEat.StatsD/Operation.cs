namespace JustEat.StatsD
{
    /// <summary>
    /// Define the Gauge operation
    /// </summary>
    public enum Operation
    {
        /// <summary>
        /// Set the gauge value
        /// </summary>
        Set,
        /// <summary>
        /// Increment the gauge value
        /// </summary>
        Increment,
        /// <summary>
        /// Decrement the gauge value
        /// </summary>
        Decrement
    }
}
