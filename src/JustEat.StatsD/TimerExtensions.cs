using System;

namespace JustEat.StatsD
{
    /// <summary>
    /// Usage:
    /// 
    ///  timing a block of code in a using statment:
    ///  using (stats.StartTimer("someStat"))
    ///  {
    ///     DoSomething();
    ///  }
    /// 
    ///  timing a lambda without a return value:
    /// stats.Time("someStat", () => DoSomething());
    /// 
    ///  timing a lambda with a return value:
    /// var result = stats.Time("someStat", () => GetSomething());
    /// </summary>
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