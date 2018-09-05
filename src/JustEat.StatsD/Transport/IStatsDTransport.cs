namespace JustEat.StatsD
{
    public interface IStatsDTransport
    {
        void Send(in Data metric);
    }
}
