using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace JustEat.StatsD
{
    /// <summary>
    /// A class containing extension methods for the <see cref="IStatsDTransport"/> interface. This class cannot be inherited.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IStatsDTransportExtensions
    {
        /// <summary>
        /// Sends the specified metrics to the statsD server.
        /// </summary>
        /// <param name="transport">The <see cref="IStatsDTransport"/> to use.</param>
        /// <param name="metrics">The metric(s) to send.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transport"/> or <paramref name="metrics"/> is <see langword="null"/>.
        /// </exception>
        public static void Send(this IStatsDTransport transport, IEnumerable<string> metrics)
        {
            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            if (metrics == null)
            {
                throw new ArgumentNullException(nameof(metrics));
            }

            foreach (string metric in metrics)
            {
                transport.Send(metric);
            }
        }

        /// <summary>
        /// Sends the specified metric to the statsD server.
        /// </summary>
        /// <param name="transport">The <see cref="IStatsDTransport"/> to use.</param>
        /// <param name="metric">The metric to send.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="transport"/> or <paramref name="metric"/> is <see langword="null"/>.
        /// </exception>
        public static void Send(this IStatsDTransport transport, string metric)
        {
            if (transport == null)
            {
                throw new ArgumentNullException(nameof(transport));
            }

            if (metric == null)
            {
                throw new ArgumentNullException(nameof(metric));
            }

            var buffer = Encoding.UTF8.GetBytes(metric);
            var segment = new ArraySegment<byte>(buffer);

            transport.Send(segment);
        }
    }
}
