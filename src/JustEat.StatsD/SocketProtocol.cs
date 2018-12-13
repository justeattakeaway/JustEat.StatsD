using System.Net.Sockets;

namespace JustEat.StatsD
{
    /// <summary>
    /// An enumeration defining the subset of <see cref="ProtocolType"/> values that are supported by <see cref="SocketTransport"/>.
    /// <para />
    /// UDP is the default, but IP transport is required for some environments such as AWS Lambda functions.
    /// </summary>
    public enum SocketProtocol
    {
        /// <summary>
        /// Transport stats over UDP sockets.
        /// </summary>
        Udp,

        /// <summary>
        /// Transport stats over IP sockets.
        /// </summary>
        IP
    }
}
