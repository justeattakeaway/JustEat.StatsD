using System;

namespace JustEat.StatsD.Extensions
{
    public static class Timing
    {
        public const int StandardDelayMilliseconds = 400;

        public static readonly TimeSpan Delta = TimeSpan.FromMilliseconds(StandardDelayMilliseconds / 2);

    }
}