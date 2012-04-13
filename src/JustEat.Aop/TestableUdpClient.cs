using System.Net.Sockets;

namespace JustEat.Aop
{
	public class TestableUdpClient : IUdpClient
	{
		private readonly UdpClient _actual;

		public TestableUdpClient(string host, int port)
		{
			_actual = new UdpClient(host, port);
		}

		public void Send(byte[] data, int length)
		{
			_actual.Send(data, length);
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
