using System;

namespace JustEat.StatsD.Extensions
{
    public static class TimingConstants
    {
        public const int DelayMilliseconds = 400;

        public static readonly TimeSpan DeltaFast = TimeSpan.FromMilliseconds(DelayMilliseconds / 4);
        public static readonly TimeSpan DeltaSlow = TimeSpan.FromMilliseconds(DelayMilliseconds);
    }
}