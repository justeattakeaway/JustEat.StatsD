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
