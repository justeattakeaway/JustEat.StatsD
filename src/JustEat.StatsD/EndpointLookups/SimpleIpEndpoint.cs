using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// Simple adapter
    /// </summary>
    public class SimpleIpEndpoint : IPEndPointSource
    {
        private readonly IPEndPoint _value;

        public SimpleIpEndpoint(IPEndPoint value)
        {
            _value = value;
        }

        public IPEndPoint GetEndpoint() => _value;
    }
}