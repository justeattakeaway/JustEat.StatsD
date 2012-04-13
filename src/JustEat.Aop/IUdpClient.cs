using System;

namespace JustEat.Aop
{
	public interface IUdpClient : IDisposable
	{
		void Send(byte[] data, int length);
	}
}
