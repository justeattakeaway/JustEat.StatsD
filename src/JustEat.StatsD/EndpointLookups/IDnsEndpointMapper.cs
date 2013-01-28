using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    public interface IDnsEndpointMapper
    {
        IPEndPoint GetIpEndPoint(string hostName, int port);
    }
}