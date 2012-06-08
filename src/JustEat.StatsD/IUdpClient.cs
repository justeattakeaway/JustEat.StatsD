using System;

namespace JustEat.StatsD
{
    public interface IUdpClient : IDisposable
    {
        void Send(byte[] data, int length);
    }
}
