using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JustEat.StatsD
{
    /// <summary>	
    /// A helper class for turning a list of strings into a Udp packet. 
    /// </summary>
    public static class PacketBuilder
    {
        /// <summary>
        /// Takes a list of metric strings, separating them with newlines 
        /// into a list of byte packet, each a maximum of 512 bytes in size.
        /// </summary>
        /// <param name="metrics">	The metrics to act on. </param>
        /// <returns>	A streamed list of byte arrays, where each array is a maximum of 512 bytes. </returns>
        public static IEnumerable<byte[]> ToMaximumBytePackets(this IEnumerable<string> metrics)
        {
            return ToMaximumBytePackets(metrics, 512);
        }

        /// <summary>
        /// Takes a list of metric strings, separating them with newlines into a byte packet of the maximum specified size.
        /// </summary>
        /// <param name="metrics">The metrics to act on.</param>
        /// <param name="packetSize">Maximum size of each packet (512 bytes recommended for Udp). </param>
        /// <returns>	A streamed list of byte arrays, where each array is a maximum of 512 bytes. </returns>
        public static IEnumerable<byte[]> ToMaximumBytePackets(this IEnumerable<string> metrics, int packetSize)
        {
            var packet = new List<byte>(packetSize);

            foreach (var metric in AddNewLineToBatchMetrics(metrics))
            {
                var bytes = Encoding.UTF8.GetBytes(metric);
                if (packet.Count + bytes.Length <= packetSize)
                {
                    packet.AddRange(bytes);
                    //	packet.AddRange(Terminator);
                }
                else if (bytes.Length >= packetSize)
                {
                    yield return bytes;
                }
                else
                {
                    yield return packet.ToArray();
                    packet.Clear();
                    packet.AddRange(bytes);
                    //	packet.AddRange(Terminator);
                }
            }

            if (packet.Count > 0)
            {
                yield return packet.ToArray();
            }
        }

        /// <summary>
        ///     Had little option but to do this, we need newlines in our batch metrics, statsd don't really explain this clearly enough, but
        ///     appending a new line to the end of a metric is not the correct way to go, you need to have newlines in between metrics but
        ///     not at the end of a packet sent to statsd, as it results in bad lines getting sent to the statsd server.
        ///     so for example metric1\nmetric2\nmetric3\n will throw a bad line error in the statsd logs, but metric1\nmetric2\nmetric3 wont.
        /// </summary>
        /// <param name="metrics"> The metrics to act on.</param>
        /// <returns>IEnumerable string list with appended terminators when we have more than one metric to send.</returns>
        private static IEnumerable<string> AddNewLineToBatchMetrics(IEnumerable<string> metrics)
        {
            IEnumerator en = metrics.GetEnumerator();
            var metricList = new List<string>();
            if (en.MoveNext())
            {
                // we have at least one item.
                metricList.Add(string.Empty + en.Current);
            }
            while (en.MoveNext())
            {
                // second and subsequent get delimiter
                const string seperator = "\n";
                metricList.Add(seperator + en.Current);
            }
            return metricList;
        }
    }
}
