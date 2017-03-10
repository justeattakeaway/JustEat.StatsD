using System;

namespace JustEat.StatsD.Extensions
{
    public static class TimingConstants
    {
        public const int DelayMilliseconds = 400;

        public static readonly TimeSpan Delta = TimeSpan.FromMilliseconds(DelayMilliseconds / 2);
    }
}