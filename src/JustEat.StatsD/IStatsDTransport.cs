namespace JustEat.StatsD
{
    public interface IStatsDTransport
    {
        void Send(string metric);
    }
}
