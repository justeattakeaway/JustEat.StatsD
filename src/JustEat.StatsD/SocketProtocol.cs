namespace JustEat.StatsD
{
    /// <summary>
    /// The subset of ProtocolType that are supported by SocketTransport
    /// UDP or IP.
    /// UDP is the default, but IP transport is required for AWS Lambdas.
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
