using System;
using System.Threading.Tasks;

namespace JustEat.StatsD
{
    public static class TimerExtensions
    {
        /// <summary>
        /// Start a timer for use in a "using" statement
        /// </summary>
        /// <param name="publisher">the stats publisher</param>
        /// <param name="bucket">the stat name</param>
        /// <returns>the disposable timer</returns>
        public static IDisposableTimer StartTimer(this IStatsDPublisher publisher, string bucket)
        {
            return new DisposableTimer(publisher, bucket);
        }

        /// <summary>
        /// functional style for timing an delegate with no return value, is not async
        /// </summary>
        /// <param name="publisher">the stats publisher</param>
        /// <param name="bucket">the stat name</param>
        /// <param name="action">the delegate to time</param>
        public static void Time(this IStatsDPublisher publisher, string bucket, Action<IDisposableTimer> action)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                action(timer);
            }
        }

        /// <summary>
        /// functional style for timing an delegate with no return value, is async
        /// </summary>
        /// <param name="publisher">the stats publisher</param>
        /// <param name="bucket">the stat name</param>
        /// <param name="action">the delegate to time</param>
        public static async Task Time(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, Task> action)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                await action(timer);
            }
        }

        /// <summary>
        /// functional style for timing a function with a return value, is not async
        /// </summary>
        /// <param name="publisher">the stats publisher</param>
        /// <param name="bucket">the stat name</param>
        /// <param name="func">the function to time</param>
        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, T> func)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                return func(timer);
            }
        }


        /// <summary>
        /// functional style for timing a function with a return value, is async
        /// </summary>
        /// <param name="publisher">the stats publisher</param>
        /// <param name="bucket">the stat name</param>
        /// <param name="func">the function to time</param>
        public static async Task<T> Time<T>(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, Task<T>> func)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                return await func(timer);
            }
        }
    }
}
