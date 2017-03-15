using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// Simple adapter
    /// </summary>
    public class SimpleIpEndpoint : IPEndPointSource
    {
        public SimpleIpEndpoint(IPEndPoint value)
        {
            Endpoint = value;
        }

        public IPEndPoint Endpoint { get; }
    }
}