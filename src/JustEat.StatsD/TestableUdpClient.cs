using System;
using System.Net.Sockets;
using System.Text;

namespace JustEat.StatsD
{
	[Serializable]
	public class TestableUdpClient : IStatsDUdpClient
	{
		private readonly UdpClient _actual;

		public TestableUdpClient(string host, int port)
		{
			_actual = new UdpClient(host, port);
		}

		public bool Send(string metric)
		{
			var data = Encoding.Default.GetBytes(metric);

			_actual.Send(data, data.Length);
			return true;
		}

		public void Dispose()
		{
			try
			{
				if (_actual != null)
				{
					_actual.Close();
				}
			}
			catch { }
		}
	}
}
