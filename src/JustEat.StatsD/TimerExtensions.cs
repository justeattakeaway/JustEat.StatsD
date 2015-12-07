using System;
using System.Threading.Tasks;

namespace JustEat.StatsD
{
    public static class TimerExtensions
    {
        public static IDisposableTimer StartTimer(this IStatsDPublisher publisher, string bucket)
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

        public static async Task Time(this IStatsDPublisher publisher, string bucket, Func<Task> action)
        {
            using (StartTimer(publisher, bucket))
            {
                await action();
            }
        }

        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<T> func)
        {
            using (StartTimer(publisher, bucket))
            {
                return func();
            }
        }

        public static async Task<T> Time<T>(this IStatsDPublisher publisher, string bucket, Func<Task<T>> func)
        {
            using (StartTimer(publisher, bucket))
            {
                return await func();
            }
        }

    }
}
