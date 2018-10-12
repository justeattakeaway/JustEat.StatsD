using System;

namespace JustEat.StatsD
{
    /// <summary>
    /// Defines a transport for sending metrics to a StatsD server.
    /// </summary>
    public interface IStatsDTransport
    {
        /// <summary>
        /// Sends the metric represented by the specified array segment to the server.
        /// </summary>
        /// <param name="metric">An <see cref="ArraySegment{T}"/> containing the bytes of the metric to send.</param>
        void Send(in ArraySegment<byte> metric);
    }
}
