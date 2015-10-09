using System;

namespace JustEat.StatsD
{
    public static class TimerExtensions
    {
        public static IDisposable StartTimer(this IStatsDPublisher publisher, string bucket)
        {
            return new DisposableTimer(publisher, bucket);
        }

        public static void Time(this IStatsDPublisher publisher, string bucket, Action action)
        {
            using (StartTimer(publisher, bucket))
            {
                action();
            }
        }

        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<T> func)
        {
            using (StartTimer(publisher, bucket))
            {
                return func();
            }
        }
    }
}