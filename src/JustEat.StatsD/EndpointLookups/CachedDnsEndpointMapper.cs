using System;
using System.Net;

namespace JustEat.StatsD.EndpointLookups
{
    /// <summary>
    ///     Looks up an endpoint and holds the ip endpoint in memory for the specified duration (seconds)
    /// </summary>
    public class CachedDnsEndpointMapper : IDnsEndpointMapper
    {
        private readonly IDnsEndpointMapper _baseMapper;
        private readonly int _secondsCacheDuration;
        private CachedEndPoint _cachedEndPoint;

        /// <summary>
        /// </summary>
        /// <param name="baseMapper">Base endpoint provider</param>
        /// <param name="secondsCacheDuration">Seconds to cache ip endpoints for</param>
        public CachedDnsEndpointMapper(IDnsEndpointMapper baseMapper, int secondsCacheDuration)
        {
            _baseMapper = baseMapper;
            _secondsCacheDuration = secondsCacheDuration;
        }

        /// <summary>
        ///     Get an IP Endpoint for the DNS host name
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public IPEndPoint GetIpEndPoint(string hostName, int port)
        {
            lock (this)
            {
                if (_cachedEndPoint == null || _cachedEndPoint.IsExpired(_secondsCacheDuration))
                {
                    _cachedEndPoint = new CachedEndPoint(_baseMapper.GetIpEndPoint(hostName, port));
                }
            }
            return _cachedEndPoint.IpEndPoint;
        }

        #region Nested type: CachedEndPoint

        internal class CachedEndPoint
        {
            public CachedEndPoint(IPEndPoint endpoint)
            {
                IpEndPoint = endpoint;
                CreateTime = DateTime.UtcNow;
            }

            internal IPEndPoint IpEndPoint { get; private set; }
            internal DateTime CreateTime { get; private set; }

            public bool IsExpired(int secondsCacheDuration)
            {
                return CreateTime.AddSeconds(secondsCacheDuration) <= DateTime.UtcNow;
            }
        }

        #endregion
    }
}
