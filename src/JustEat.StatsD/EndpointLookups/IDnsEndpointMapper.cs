using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public interface IDnsEndpointMapper
    {
        IPEndPoint GetIPEndPoint(string hostName, int port);
    }
}
