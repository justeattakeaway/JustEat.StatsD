using System;
using System.Net.Sockets;
using System.Text;

namespace JustEat.StatsD
{
	public class StatsDUdpClient : IStatsDUdpClient
	{
		private readonly UdpClient _udpClient;

		public StatsDUdpClient(string host, int port)
		{
			_udpClient = new UdpClient(host, port);
		}

		public bool Send(string metric)
		{
			var data = Encoding.Default.GetBytes(metric);

			_udpClient.Send(data, data.Length);

			return true;
		}

		public void Dispose()
		{
			try
			{
				if (_udpClient != null)
				{
					_udpClient.Close();
					GC.SuppressFinalize(this);
				}
			}
			catch (Exception)
			{

			}
		}
	}
}
