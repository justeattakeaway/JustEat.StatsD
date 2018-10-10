using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    /// Simple adapter
    /// </summary>
    public class SimpleEndpointSource : IEndPointSource
    {
        private readonly EndPoint _value;

        public SimpleEndpointSource(EndPoint value)
        {
            _value = value;
        }

        public EndPoint GetEndpoint() => _value;
    }
}
