using System;

namespace JustEat.StatsD.Extensions
{
    public static class TimingConstants
    {
        public const int DelayMilliseconds = 500;

        public static readonly TimeSpan Delta = TimeSpan.FromMilliseconds(DelayMilliseconds / 2);
    }
}