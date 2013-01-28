using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using JustEat.StatsD.EndpointLookups;

namespace JustEat.StatsD
{
	public class StatsDUdpClient : IStatsDUdpClient
	{
		private readonly UdpClient _udpClient;
		private bool _disposed;
		private readonly string _hostNameOrAddress;
		private readonly int _port;
		private readonly IPEndPoint _ipBasedEndpoint;
		private readonly static SimpleObjectPool<SocketAsyncEventArgs> EventArgsPool 
			= new SimpleObjectPool<SocketAsyncEventArgs>(30, pool => new PoolAwareSocketAsyncEventArgs(pool));
	    private readonly IDnsEndpointMapper _endPointMapper;

		public StatsDUdpClient(string hostNameOrAddress, int port)
            : this(new DnsEndpointProvider(), hostNameOrAddress, port)
		{ }

	    public StatsDUdpClient(int endpointCacheDuration, string hostNameOrAddress, int port)
            : this(new CachedDnsEndpointMapper(new DnsEndpointProvider(), endpointCacheDuration), hostNameOrAddress, port)
	    { }

        private StatsDUdpClient(IDnsEndpointMapper endpointMapper, string hostNameOrAddress, int port)
        {
            _endPointMapper = endpointMapper;
			_hostNameOrAddress = hostNameOrAddress;
			_port = port;
			_udpClient = new UdpClient(_hostNameOrAddress, _port)
			             {
			             	Client = {SendBufferSize = 0}
			             };

			//if we were given an IP instead of a hostname, we can happily cache it off for the life of this class
			IPAddress address;
			if (IPAddress.TryParse(hostNameOrAddress, out address))
			{
				_ipBasedEndpoint = new IPEndPoint(address, _port);
			}
        }

	    public bool Send(string metric)
		{
			return Send(new[] { metric });
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This is one of the rare cases where eating exceptions is OK")]
		public bool Send(IEnumerable<string> metrics)
		{
			var data = EventArgsPool.Pop();
			//firehose alert! -- keep it moving!
			if (null == data) { return false; }

			try
			{
				data.RemoteEndPoint = _ipBasedEndpoint ?? _endPointMapper.GetIpEndPoint(_hostNameOrAddress, _port); //only DNS resolve if we were given a hostname
				data.SendPacketsElements = metrics.ToMaximumBytePackets()
					.Select(bytes => new SendPacketsElement(bytes, 0, bytes.Length, true))
					.ToArray();
				
				_udpClient.Client.SendPacketsAsync(data);

				return true;
			}
			//fire and forget, so just eat intermittent failures / exceptions
			catch (Exception) 
			{ }

			return false;
		}

		/// <summary>	Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
		public void Dispose()
		{
			if (!_disposed)
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>	Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources. </summary>
		/// <param name="disposing">	true if resources should be disposed, false if not. </param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != _udpClient)
				{
					_udpClient.Close();
				}
				_disposed = true;
			}
		}
	}
}
