using System;
using System.Threading.Tasks;

namespace JustEat.StatsD
{
    public static class TimerExtensions
    {
        /// <summary>
        /// Start a timer for use in a "using" statement
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <returns></returns>
        public static IDisposableTimer StartTimer(this IStatsDPublisher publisher, string bucket)
        {
            return new DisposableTimer(publisher, bucket);
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: no
        /// async: no
        /// access to the timer name: no
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="action"></param>
        public static void Time(this IStatsDPublisher publisher, string bucket, Action action)
        {
            using (StartTimer(publisher, bucket))
            {
                action();
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: no
        /// async: no
        /// access to the timer name: yes
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="action"></param>
        public static void Time(this IStatsDPublisher publisher, string bucket, Action<IDisposableTimer> action)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                action(timer);
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: no
        /// async: yes
        /// access to the timer name: no
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="action"></param>
        public static async Task Time(this IStatsDPublisher publisher, string bucket, Func<Task> action)
        {
            using (StartTimer(publisher, bucket))
            {
                await action();
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: no
        /// async: yes
        /// access to the timer name: yes
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="action"></param>
        public static async Task Time(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, Task> action)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                await action(timer);
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: yes
        /// async: no
        /// access to the timer name: no
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="func"></param>
        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<T> func)
        {
            using (StartTimer(publisher, bucket))
            {
                return func();
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: yes
        /// async: no
        /// access to the timer name: yes
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="func"></param>
        public static T Time<T>(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, T> func)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                return func(timer);
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: yes
        /// async: yes
        /// access to the timer name: no
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="func"></param>
        public static async Task<T> Time<T>(this IStatsDPublisher publisher, string bucket, Func<Task<T>> func)
        {
            using (StartTimer(publisher, bucket))
            {
                return await func();
            }
        }

        /// <summary>
        /// functional style for timing a lambda with
        /// return value: yes
        /// async: yes
        /// access to the timer name: yes
        /// </summary>
        /// <param name="publisher"></param>
        /// <param name="bucket"></param>
        /// <param name="func"></param>
        public static async Task<T> Time<T>(this IStatsDPublisher publisher, string bucket, Func<IDisposableTimer, Task<T>> func)
        {
            using (var timer = StartTimer(publisher, bucket))
            {
                return await func(timer);
            }
        }
    }
}
