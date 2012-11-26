using System;

namespace JustEat.StatsD
{
	public interface IStatsDUdpClient : IDisposable
	{
		bool Send(string metric);
	}
}
