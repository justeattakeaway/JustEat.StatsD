using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// Simple adapter
    /// </summary>
    public class SimpleIpEndpoint : IEndPointSource
    {
        private readonly EndPoint _value;

        public SimpleIpEndpoint(EndPoint value)
        {
            _value = value;
        }

        public EndPoint GetEndpoint() => _value;
    }
}
